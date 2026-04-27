using renameafter.Models.DTOs;
using renameafter.Models.Entities;
using renameafter.Services;

namespace renameafter.Interfaces
{

    
    public interface IAuthService
    {
        Task<AuthResult> SignupUserAsync(SignupDto model);
        Task<AuthResult> SigninAsync(SigninDto model);
        Task<AuthResult> ConfirmEmailAsync(ConfirmEmailDto model);
        Task<AuthResult> ForgotPasswordAsync(ForgotPasswordDto model);
        Task<AuthResult> ResetPasswordAsync(ResetPasswordDto model);
        Task<AuthResult> ChangePasswordAsync(ChangePasswordDto model, string userId);
        Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken);
    }
}



