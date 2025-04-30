using System;
using System.Text.Json.Serialization;

namespace SocialAppLibrary.GotShared.Dtos
{
    //#25 nếu được muốn nâng cấp thành Cách 2: Hỗ trợ cả Email và Username
    public record LoginDto(string Email, string Password);

    
}
