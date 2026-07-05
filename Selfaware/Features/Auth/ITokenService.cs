using System.Security.Claims;
using Selfaware.Features.User.Entities;

namespace Selfaware.Features.Auth
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
