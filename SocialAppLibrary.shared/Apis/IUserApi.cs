using Microsoft.AspNetCore.Http;
using Refit;
using SocialAppLibrary.GotShared.Dtos;

namespace SocialAppLibrary.GotShared.Apis;

//[Headers("Authorization: Bearer ")]
public interface IUserApi
{
    [Post("/api/user/change-photo")]
    Task<ApiResult<string>> ChangePhotoAsync(IFormFile photo);
    [Get("/api/user/posts")]
    Task<PostDto[]> GetUserPostsAsync(int startIndex, int pageSize);
    [Get("/api/user/bookmarked-posts")]
    Task<PostDto[]> GetUserBookmarkedPostsAsync(int startIndex, int pageSize);
}
