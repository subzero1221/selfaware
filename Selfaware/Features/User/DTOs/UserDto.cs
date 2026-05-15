

namespace Selfaware.Features.User.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }

    }
}
