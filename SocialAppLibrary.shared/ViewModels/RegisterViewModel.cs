using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SocialAppLibrary.GotShared.Apis;
using SocialAppLibrary.GotShared.Services;
using SocialAppLibrary.GotShared.Dtos;
using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;

namespace SocialAppLibrary.GotShared.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthApi _authApi;
        private readonly AuthService _authService;
        private readonly ILogger<RegisterViewModel> _logger;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        public RegisterViewModel(
            IAuthApi authApi,
            AuthService authService,
            ILogger<RegisterViewModel> logger)
            : base(authService, logger)
        {
            _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("RegisterViewModel initialized");
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            //// Validate input
            //if (!ValidateInput(out string validationError))
            //{
            //    await ToastAsync(validationError);
            //    _logger.LogWarning("Validation failed: {ErrorMessage}", validationError);
            //    return;
            //}

            await MakeApiCall(async () =>
            {
                _logger.LogInformation("Starting registration for email={Email}, name={Name}", Email, Name);

                var registerRequest = new RegisterDto(Name, Email, Password);
                var result = await _authApi.RegisterAsync(registerRequest);

                _logger.LogInformation("API response: {Result}", JsonSerializer.Serialize(result));

                if (result?.IsSuccess == true && result.Data != null)
                {
                    _logger.LogInformation("Registration successful for email={Email}", Email);
                    await ToastAsync("Registration successful!");

                    // Clear password for security
                    Password = string.Empty;

                    await NavigationAsync("//loginPage");
                    _logger.LogInformation("Navigated to loginPage");
                }
                else
                {
                    var errorMessage = result?.Error ?? "Failed to register account.";
                    _logger.LogError("Registration failed: {ErrorMessage}", errorMessage);
                    throw new Exception(errorMessage);
                }
            });
        }

        private bool ValidateInput(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2 || Name.Length > 50)
            {
                errorMessage = "Name must be between 2 and 50 characters.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email))
            {
                errorMessage = "Invalid email format.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
            {
                errorMessage = "Password must be at least 8 characters long.";
                return false;
            }

            if (!Password.Any(char.IsLetter) || !Password.Any(char.IsDigit))
            {
                errorMessage = "Password must contain at least one letter and one number.";
                return false;
            }

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
    }
}