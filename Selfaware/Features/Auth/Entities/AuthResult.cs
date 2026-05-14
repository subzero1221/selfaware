using Microsoft.AspNetCore.Identity;

namespace Selfaware.Features.Auth.Entities
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public IEnumerable<IdentityError> Errors { get; set; }
        public string ErrorMessage { get; set; }

        public string Message { get; set; }

        public string? RefreshToken { get; set; }
    }
}
