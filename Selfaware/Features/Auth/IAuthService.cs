using Selfaware.Features.Auth.DTOs;
using Selfaware.Features.Auth.Entities;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Auth
{

    
    public interface IAuthService
    {
        Task<ServiceResult<AuthResult>> SignupUserAsync(SignupDto model);
        Task<ServiceResult<AuthResult>> SigninAsync(SigninDto model);
        Task<ServiceResult<AuthResult>> ConfirmEmailAsync(ConfirmEmailDto model);
        Task<ServiceResult<AuthResult>> ForgotPasswordAsync(ForgotPasswordDto model);
        Task<ServiceResult<AuthResult>> ResetPasswordAsync(ResetPasswordDto model);
        Task<ServiceResult<AuthResult>> ChangePasswordAsync(ChangePasswordDto model, string userId);
        Task<ServiceResult<AuthResult>> RefreshTokenAsync(string accessToken, string refreshToken);
    }
}



