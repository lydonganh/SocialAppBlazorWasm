using System.Text.Json.Serialization;

namespace SocialAppLibrary.GotShared.Dtos
{
    public record LoggedInUser(
    [property: JsonPropertyName("id")] Guid ID,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("photoUrl")] string? PhotoUrl,
    [property: JsonPropertyName("token")] string Token // chỗ này có cần ko 

        );
}
