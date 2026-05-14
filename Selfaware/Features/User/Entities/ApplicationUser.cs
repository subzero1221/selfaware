using Microsoft.AspNetCore.Identity;

namespace Selfaware.Features.User.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public string? ZodiacSign { get; set; }
        public DateTime BirthDate { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        public bool IsActive { get; set; }
    }
}
