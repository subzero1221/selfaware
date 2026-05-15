

namespace Selfaware.Features.Auth.Entities
{
    public class AuthResult
    {
        public string Token { get; set; }

        public string? RefreshToken { get; set; }
    }
}
