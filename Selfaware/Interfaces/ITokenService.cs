using Selfaware.Models.Entities;
using System.Security.Claims;
namespace Selfaware.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
