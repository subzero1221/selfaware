using Microsoft.AspNetCore.Identity;

namespace Selfaware.Features.User.Entities
{
    public class ApplicationUser: IdentityUser
    {

        public DateTime BirthDate { get; set; }

        public string DisplayName { get; set; }

        public string ?Bio { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
