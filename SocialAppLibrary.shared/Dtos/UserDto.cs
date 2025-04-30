namespace SocialAppLibrary.GotShared.Dtos
{
    // UserDto.cs
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhotoUrl { get; set; } = default!;
    }
}
