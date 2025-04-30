// Nhập namespace cần thiết cho JSON serialization
using System.Text.Json.Serialization; // Cung cấp thuộc tính JsonPropertyName để ánh xạ JSON

namespace SocialAppLibrary.GotShared.Dtos
{
    /// <summary>
    /// Record LoginResponse biểu diễn phản hồi từ API đăng nhập.
    /// Chứa thông tin người dùng, access token và refresh token.
    /// </summary>
    public record LoginResponse(
        [property: JsonPropertyName("user")] LoggedInUser? User,
        [property: JsonPropertyName("accessToken")] string? Token,
        [property: JsonPropertyName("refreshToken")] string? RefreshToken
    );
}