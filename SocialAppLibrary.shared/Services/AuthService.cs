using Microsoft.Extensions.Logging;
using SocialAppLibrary.GotShared.Apis;
using SocialAppLibrary.GotShared.Dtos;
using System.Diagnostics;
using System.Text.Json;

namespace SocialAppLibrary.GotShared.Services
{
    public class AuthService
    {
        private const string UserDataKey = "udata";
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public LoggedInUser? User { get; private set; }

        public bool IsLoggedIn => User is not null && User.ID != default && !string.IsNullOrWhiteSpace(Token);

        private readonly HttpClient _httpClient;
        private readonly AppPreferences _appPreferences;
        private readonly IAuthApi _authApi;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            HttpClient httpClient,
            AppPreferences appPreferences,
            IAuthApi authApi,
            ILogger<AuthService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _appPreferences = appPreferences ?? throw new ArgumentNullException(nameof(appPreferences));
            _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("AuthService initialized");
        }

        public async Task Login(LoginResponse loginResponse)
        {
            if (loginResponse?.User == null)
                throw new ArgumentNullException(nameof(loginResponse.User));

            User = loginResponse.User;
            Token = loginResponse.Token;
            RefreshToken = loginResponse.RefreshToken;
            //var json = JsonSerializer.Serialize(loginResponse);
            ////Preferences.Default.Set(UserDataKey, json);
            //////await _appPreferences.SaveLoginResponseAsync(loginResponse);
            ////// ✅ Thêm dòng này
            //await _appPreferences.SaveUserInfoAsync(User);

            await _appPreferences.SaveLoginResponseAsync(loginResponse);
            _logger.LogInformation("Token set: {Token}, RefreshToken: {RefreshToken}", Token, RefreshToken);
        }

        public async Task Logout()
        {
            (User, Token, RefreshToken) = (null, null, null);
            Preferences.Default.Remove(UserDataKey);
            _appPreferences.Remove(UserDataKey);
            _logger.LogInformation("Logged out");
        }

        public async Task InitializeAsync()
        {
            User = await _appPreferences.GetUserInfoAsync();
            var udata = Preferences.Default.Get<string?>(UserDataKey, null);
            if (!string.IsNullOrWhiteSpace(udata))
            {
                try
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(udata);
                    if (loginResponse != null && loginResponse.User is not null && loginResponse.User.ID != default)
                    {
                        User = loginResponse.User;
                        Token = loginResponse.Token;
                        RefreshToken = loginResponse.RefreshToken;
                    }
                    else
                    {
                        Preferences.Default.Remove(UserDataKey);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing user data");
                    Preferences.Default.Remove(UserDataKey);
                }
            }
        }

        public async Task SetUserInfoAsync()
        {
            var user = await _appPreferences.GetUserInfoAsync();
            if (user == null)
            {
                _logger.LogWarning("UserInfo is null. Possibly old or invalid JSON.");
                return;
            }

            Token = user.Token;
            _logger.LogInformation("Token set: {Token}", Token);
        }

        public async Task<bool> RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(RefreshToken))
            {
                _logger.LogWarning("Refresh token does not exist.");
                return false;
            }

            try
            {
                var refreshTokenDto = new RefreshTokenDto(RefreshToken);
                var result = await _authApi.RefreshTokenAsync(refreshTokenDto);
                if (!result.IsSuccess)
                {
                    _logger.LogError("Failed to refresh token: {Error}", result.Error);
                    return false;
                }

                await Login(result.Data);
                _logger.LogInformation("Token refreshed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return false;
            }
        }
    }
}