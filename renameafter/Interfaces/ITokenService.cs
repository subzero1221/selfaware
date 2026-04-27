using renameafter.Models.Entities;
using System.Security.Claims;
namespace renameafter.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
