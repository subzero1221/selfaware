using Selfaware.Features.User.Entities;
using System.Security.Claims;
namespace Selfaware.Features.Auth
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
