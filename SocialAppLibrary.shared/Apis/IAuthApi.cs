using Microsoft.AspNetCore.Http;
using Refit;
using SocialAppLibrary.GotShared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialAppLibrary.GotShared.Apis;

public interface IAuthApi
{
    
    [Multipart]
    [Post("/api/auth/register/{userId}/upload-photo")]
    Task<ApiResult<string>> UploadPhotoAsync(Guid userId, IFormFile photo);

    [Post("/api/auth/register")]
    Task<ApiResult<RegisterResponse>> RegisterAsync([Body] RegisterDto request);

    [Post("/api/auth/login")]
    Task<ApiResult<LoginResponse>> LoginAsync(LoginDto dto);

    [Post("/api/auth/refresh")]
    Task<ApiResult<LoginResponse>> RefreshTokenAsync([Body] RefreshTokenDto refreshTokenDto);

}
