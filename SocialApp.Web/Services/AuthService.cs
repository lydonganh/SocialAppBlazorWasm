using SocialAppLibrary.GotShared.Dtos;
using System.Net.Http.Json;

namespace SocialApp.Web.Services
{
    // AuthService.cs
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            try
            {
                var loginDto = new LoginDto(email, password);
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (loginResponse != null)
                    {
                        return loginResponse;
                    }
                }
            }
            catch (Exception)
            {
                // Optionally log error
            }

            // Return a default or throw an exception to ensure non-nullability
            throw new InvalidOperationException("Login failed and no valid response was returned.");
        }

    }

}
