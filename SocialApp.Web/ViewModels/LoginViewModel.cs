using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SocialApp.Web.Services;
using SocialAppLibrary.GotShared.Dtos;

public class LoginViewModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public bool IsLoading { get; set; }
    public string ErrorMessage { get; set; } = "";

    public IAuthService? AuthService { get; set; }
    public NavigationManager? Navigation { get; set; }
    public ILocalStorageService? LocalStorage { get; set; } // 👈 Thêm dòng này

    public async Task LoginAsync()
    {
        if (AuthService is null || Navigation is null || LocalStorage is null)
        {
            ErrorMessage = "Internal error.";
            return;
        }

        IsLoading = true;
        ErrorMessage = "";

        var response = await AuthService.LoginAsync(Email, Password);

        IsLoading = false;

        if (response != null && !string.IsNullOrEmpty(response.Token))
        {
            // Lưu AccessToken
            await LocalStorage.SetItemAsync("AccessToken", response.Token);

            // Lưu RefreshToken
            if (!string.IsNullOrEmpty(response.RefreshToken))
            {
                await LocalStorage.SetItemAsync("RefreshToken", response.RefreshToken);
            }

            // Lưu User Info
            if (response.User != null)
            {
                await LocalStorage.SetItemAsync("UserName", response.User.Name);
                await LocalStorage.SetItemAsync("UserEmail", response.User.Email);
                await LocalStorage.SetItemAsync("UserId", response.User.ID.ToString());

                if (!string.IsNullOrEmpty(response.User.PhotoUrl))
                {
                    await LocalStorage.SetItemAsync("UserPhoto", response.User.PhotoUrl);
                }
            }

            Navigation.NavigateTo("/Home");
        }
        else
        {
            ErrorMessage = "Login failed.";
        }
    }
}
