using SocialAppLibrary.GotShared.Dtos;

namespace SocialApp.Web.Services
{
    // IAuthService.cs
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
    }

}
