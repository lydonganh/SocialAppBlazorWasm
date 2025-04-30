using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;
using SocialAppLibrary.GotShared.Apis;
using SocialAppLibrary.GotShared.Services;
using SocialAppLibrary.GotShared.Dtos;

namespace SocialAppLibrary.GotShared.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthApi _authApi;
        private readonly AuthService _authService;
        private readonly AppPreferences _appPreferences;
        private readonly ILogger<LoginViewModel> _logger;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage;

        private bool _rememberMe;
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public LoginViewModel(
            IAuthApi authApi,
            AuthService authService,
            AppPreferences appPreferences,
            ILogger<LoginViewModel> logger)
            : base(authService, logger)
        {
            _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _appPreferences = appPreferences ?? throw new ArgumentNullException(nameof(appPreferences));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            RememberMe = _appPreferences.GetBool("RememberLogin", false);
            _logger.LogInformation("LoginViewModel initialized with RememberMe={RememberMe}", RememberMe);
        }
        partial void OnIsBusyChanged(bool value)
        {
            _logger.LogInformation("IsBusy changed to {Value}", value);
        }



        //[RelayCommand]
        //private async Task LoginAsync()
        //{
        //    if (IsBusy) return;
        //    IsBusy = true; // Đặt trước khi gọi API
        //    ErrorMessage = string.Empty;
        //    try { 
        //    // Validate input
        //    if (!ValidateInput(out string validationError))
        //    {
        //        ErrorMessage = validationError;
        //        await ToastAsync(ErrorMessage);
        //        _logger.LogWarning("Validation failed: {ErrorMessage}", ErrorMessage);
        //        return;
        //    }

        //    await MakeApiCall(async () =>
        //    {
        //        _logger.LogInformation("Attempting login for email={Email}", Email);

        //        var loginDto = new LoginDto(Email, Password);
        //        try
        //        {
        //            var result = await _authApi.LoginAsync(loginDto);
        //            _logger.LogInformation("API response: IsSuccess={IsSuccess}, Error={Error}, Data={Data}",
        //                result.IsSuccess, result.Error, result.Data);

        //            if (!result.IsSuccess)
        //            {
        //                ErrorMessage = result.Error ?? "Login failed.";
        //                _logger.LogError("Login failed: {ErrorMessage}", ErrorMessage);
        //                throw new Exception(ErrorMessage);
        //            }

        //            await _authService.Login(result.Data);
        //            _logger.LogInformation("Login successful for email={Email}", Email);

        //            _appPreferences.SetBool("RememberLogin", RememberMe);
        //            _logger.LogInformation("RememberLogin set to {RememberMe}", RememberMe);

        //            Password = string.Empty;
        //            await NavigationAsync("//HomePage");
        //            _logger.LogInformation("Navigated to HomePage");
        //        }
        //        catch (ApiException ex)
        //        {
        //            _logger.LogError(ex, "API call failed: StatusCode={StatusCode}, Content={Content}",
        //                ex.StatusCode, ex.Content);
        //            ErrorMessage = ex.Content ?? "Failed to connect to the server.";
        //            throw new Exception(ErrorMessage, ex);
        //        }
        //    });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "LoginAsync failed");
        //        ErrorMessage = ex.Message;
        //        await ToastAsync(ErrorMessage);
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //        _logger.LogInformation("LoginAsync completed");
        //    }
        //}
        //[RelayCommand]
        //private async Task LoginAsync()
        //{
        //    if (IsBusy) return;
        //    IsBusy = true;
        //    ErrorMessage = string.Empty;

        //    try
        //    {
        //        if (!ValidateInput(out string validationError))
        //        {
        //            ErrorMessage = validationError;
        //            await ToastAsync(ErrorMessage);
        //            _logger.LogWarning("Validation failed: {ErrorMessage}", ErrorMessage);
        //            return;
        //        }

        //        await MakeApiCall(async () =>
        //        {
        //            _logger.LogInformation("Attempting login for email={Email}", Email);

        //            var loginDto = new LoginDto(Email, Password);
        //            try
        //            {
        //                // Gọi API trực tiếp để lấy phản hồi thô
        //                var httpClient = new HttpClient();
        //                httpClient.BaseAddress = new Uri("https://v864p92g-7175.asse.devtunnels.ms");
        //                var requestContent = new StringContent(
        //                    System.Text.Json.JsonSerializer.Serialize(loginDto),
        //                    System.Text.Encoding.UTF8,
        //                    "application/json");
        //                var response = await httpClient.PostAsync("/api/auth/login", requestContent);
        //                var responseContent = await response.Content.ReadAsStringAsync();
        //                _logger.LogInformation("Raw API response: {ResponseContent}", responseContent);

        //                var result = await _authApi.LoginAsync(loginDto);
        //                _logger.LogInformation("Deserialized API response: IsSuccess={IsSuccess}, Error={Error}, Data={Data}",
        //                    result.IsSuccess, result.Error, result.Data);

        //                if (!result.IsSuccess)
        //                {
        //                    ErrorMessage = result.Error ?? "Login failed.";
        //                    _logger.LogError("Login failed: {ErrorMessage}", ErrorMessage);
        //                    throw new Exception(ErrorMessage);
        //                }

        //                await _authService.Login(result.Data);
        //                _logger.LogInformation("Login successful for email={Email}", Email);

        //                _appPreferences.SetBool("RememberLogin", RememberMe);
        //                _logger.LogInformation("RememberLogin set to {RememberMe}", RememberMe);

        //                Password = string.Empty;
        //                await NavigationAsync("//HomePage");
        //                _logger.LogInformation("Navigated to HomePage");
        //            }
        //            catch (ApiException ex)
        //            {
        //                _logger.LogError(ex, "API call failed: StatusCode={StatusCode}, Content={Content}",
        //                    ex.StatusCode, ex.Content);
        //                ErrorMessage = ex.Content ?? "Failed to connect to the server.";
        //                throw new Exception(ErrorMessage, ex);
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "LoginAsync failed");
        //        ErrorMessage = ex.Message;
        //        await ToastAsync(ErrorMessage);
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //        _logger.LogInformation("LoginAsync completed");
        //    }
        //}

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (!ValidateInput(out string validationError))
                {
                    ErrorMessage = validationError;
                    await ToastAsync(ErrorMessage);
                    _logger.LogWarning("Validation failed: {ErrorMessage}", ErrorMessage);
                    return;
                }

                await MakeApiCall(async () =>
                {
                    _logger.LogInformation("Attempting login for email={Email}", Email);

                    var loginDto = new LoginDto(Email, Password);
                    try
                    {
                        var result = await _authApi.LoginAsync(loginDto);
                        
                        if (result == null)
                        {
                            ErrorMessage = "Login failed: Empty response from server.";
                            _logger.LogError("Login failed: {ErrorMessage}", ErrorMessage);
                            throw new Exception(ErrorMessage);
                        }
                        // trong service thi tra ve return ApiResult<LoginResponse>.Success(loginResponse);
                        _logger.LogInformation("API response: IsSuccess={IsSuccess}, Error={Error}, UserId={UserId}, Token={Token}, RefreshToken={RefreshToken}",
                            result.IsSuccess, result.Error, result.Data?.User?.ID, result.Data?.Token, result.Data?.RefreshToken);
                        if (!result.IsSuccess)
                        {
                            ErrorMessage = result.Error ?? "Login failed.";
                            _logger.LogError("Login failed: {ErrorMessage}", ErrorMessage);
                            throw new Exception(ErrorMessage);
                        }

                        if (result.Data?.User == null || string.IsNullOrEmpty(result.Data.Token))
                        {
                            ErrorMessage = "Login failed: Invalid response from server.";
                            _logger.LogError("Login failed: {ErrorMessage}", ErrorMessage);
                            throw new Exception(ErrorMessage);
                        }

                        await _authService.Login(result.Data);
                        _logger.LogInformation("Login successful for email={Email}", Email);
                        try
                        {
                            await Shell.Current.GoToAsync("//HomePage");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Navigation error: {ex.Message}");
                        }
                        _logger.LogInformation("Navigated to HomePage");
                        _appPreferences.SetBool("RememberLogin", RememberMe);
                        _logger.LogInformation("RememberLogin set to {RememberMe}", RememberMe);

                        Password = string.Empty;
                        
                    }
                    catch (ApiException ex)
                    {

                        _logger.LogError(ex, "API call failed: StatusCode={StatusCode}, Content={Content}",
      ex.StatusCode, ex.Content); // `Content` đã là raw JSON string

                        var rawResponse = ex.Content; // ✅ Đây là raw JSON string

                        _logger.LogError("Raw response: {RawResponse}", rawResponse);

                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync failed");
                ErrorMessage = ex.Message;
                await ToastAsync(ErrorMessage);
            }
            finally
            {
                IsBusy = false;
                _logger.LogInformation("LoginAsync completed");
                try
                {
                    await Shell.Current.GoToAsync("//HomePage");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Navigation error: {ex.Message}");
                }


                _logger.LogInformation("Navigated to HomePage");
            }
        }
        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            ErrorMessage = string.Empty;
            await ShowErrorAlertAsync("Redirecting to password reset page...");
            _logger.LogInformation("Navigating to forgot password page");
            // TODO: Implement navigation to password reset page
        }

        [RelayCommand]
        private async Task SignUpAsync()
        {
            ErrorMessage = string.Empty;
            await NavigationAsync("//SignUpPage");
            _logger.LogInformation("Navigated to SignUpPage");
        }

        private bool ValidateInput(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                errorMessage = "Email and password are required.";
                return false;
            }

            //if (!IsValidEmail(Email))
            //{
            //    errorMessage = "Invalid email format.";
            //    return false;
            //}

            //if (Password.Length < 8)
            //{
            //    errorMessage = "Password must be at least 8 characters long.";
            //    return false;
            //}

            //if (!Password.Any(char.IsLetter) || !Password.Any(char.IsDigit))
            //{
            //    errorMessage = "Password must contain at least one letter and one number.";
            //    return false;
            //}

            return true;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 100)
            {
                _logger.LogWarning("Invalid email: {Email}", email);
                return false;
            }

            try
            {
                var addr = new MailAddress(email);
                bool isValid = addr.Address == email;
                _logger.LogInformation("Email validation: {Email} -> Valid={IsValid}", email, isValid);
                return isValid;
            }
            catch
            {
                _logger.LogWarning("Invalid email format: {Email}", email);
                return false;
            }
        }
        #region other methods
        [RelayCommand]
        private async Task GoogleSignInAsync()
        {
            _logger.LogInformation("Google Sign-In initiated");
            // TODO: Implement Google Sign-In logic
            await ShowErrorAlertAsync("Google Sign-In not implemented yet.");
        }

        [RelayCommand]
        private async Task FacebookSignInAsync()
        {
            _logger.LogInformation("Facebook Sign-In initiated");
            // TODO: Implement Facebook Sign-In logic
            await ShowErrorAlertAsync("Facebook Sign-In not implemented yet.");
        }

        [RelayCommand]
        private async Task AppleSignInAsync()
        {
            _logger.LogInformation("Apple Sign-In initiated");
            // TODO: Implement Apple Sign-In logic
            await ShowErrorAlertAsync("Apple Sign-In not implemented yet.");
        }
        #endregion
    }
}