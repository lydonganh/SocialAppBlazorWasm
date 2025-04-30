using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;
using Refit;
using SocialAppLibrary.GotShared.Services;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SocialAppLibrary.GotShared.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        protected readonly ILogger<BaseViewModel> _logger;
        protected readonly AuthService _authService;

        public BaseViewModel(AuthService authService, ILogger<BaseViewModel> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("BaseViewModel initialized");
        }

        protected async Task ShowErrorAlertAsync(string message)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
                });
                _logger.LogWarning("Displayed error alert: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to display error alert: {Message}", message);
            }
        }

        protected async Task NavigationAsync(string url)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync(url, animate: true);
                });
                _logger.LogInformation("Navigated to: {Url}", url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Navigation failed: {Url}", url);
                await ShowErrorAlertAsync("Unable to navigate.");
            }
        }

        protected async Task NavigateBackAsync()
        {
            await NavigationAsync("..");
            _logger.LogInformation("Navigated back");
        }

        protected async Task ToastAsync(string message)
        {
            try
            {
                var toast = Toast.Make(message, ToastDuration.Short);
                await toast.Show();
                _logger.LogInformation("Displayed toast: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to display toast: {Message}", message);
            }
        }

        protected async Task MakeApiCall(Func<Task> apiCall)
        {
            if (IsBusy)
            {
                _logger.LogWarning("API call skipped: ViewModel is busy");
                return;
            }

            IsBusy = true;
            try
            {
                await apiCall.Invoke();
                _logger.LogInformation("API call completed successfully");
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized error (401), attempting to refresh token");
                var refreshed = await _authService.RefreshTokenAsync();
                if (refreshed)
                {
                    _logger.LogInformation("Token refreshed successfully, retrying API call");
                    await apiCall.Invoke();
                }
                else
                {
                    _logger.LogError("Failed to refresh token, logging out");
                    await _authService.Logout();
                    await ShowErrorAlertAsync("Session expired. Please log in again.");
                    await NavigationAsync("//loginPage");
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error: {StatusCode} - {Message}", ex.StatusCode, ex.Message);
                await ShowErrorAlertAsync($"Server error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
                await ShowErrorAlertAsync("An unexpected error occurred.");
            }
            finally
            {
                IsBusy = false;
                _logger.LogInformation("API call finished, IsBusy=false");
            }
        }

        protected async Task<T> MakeApiCall<T>(Func<Task<T>> apiCall)
        {
            if (IsBusy)
            {
                _logger.LogWarning("API call skipped: ViewModel is busy");
                return default;
            }

            IsBusy = true;
            try
            {
                var result = await apiCall.Invoke();
                _logger.LogInformation("API call completed successfully with result");
                return result;
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized error (401), attempting to refresh token");
                var refreshed = await _authService.RefreshTokenAsync();
                if (refreshed)
                {
                    _logger.LogInformation("Token refreshed successfully, retrying API call");
                    return await apiCall.Invoke();
                }
                else
                {
                    _logger.LogError("Failed to refresh token, logging out");
                    await _authService.Logout();
                    await ShowErrorAlertAsync("Session expired. Please log in again.");
                    await NavigationAsync("//loginPage");
                    return default;
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error: {StatusCode} - {Message}", ex.StatusCode, ex.Message);
                await ShowErrorAlertAsync($"Server error: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
                await ShowErrorAlertAsync("An unexpected error occurred.");
                return default;
            }
            finally
            {
                IsBusy = false;
                _logger.LogInformation("API call finished, IsBusy=false");
            }
        }
    }
}