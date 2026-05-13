using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Selfaware.Data;
using Selfaware.Interfaces;
using Selfaware.Models.DTOs;
using Selfaware.Models.Entities;
using System.Security.Claims;
using System.Text;


namespace Selfaware.Services
{

    public class AuthService : IAuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;


        public AuthService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ITokenService tokenService, IEmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<AuthResult> SignupUserAsync(SignupDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                ZodiacSign = model.ZodiacSign
            };

            string password = model.Password;

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = result.Errors
                };
            }

            await _userManager.AddToRoleAsync(user, "User");

          

            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken =  _tokenService.CreateToken(user, roles);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);


            byte[] tokenGeneratedBytes = Encoding.UTF8.GetBytes(token);
            var code = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);


            var callbackUrl = $"https://localhost:3000/confirm-email?userId={user.Id}&code={code}";


            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your Stars",
                $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>."
            );

            return new AuthResult
            {
                Success = true,
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResult> SigninAsync(SigninDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new AuthResult { Success = false, ErrorMessage = "Email or Password incorrect!" };
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return new AuthResult { Success = false, ErrorMessage = "Email or Password incorrect!" };


            var refreshToken = _tokenService.GenerateRefreshToken();

         
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.CreateToken(user, roles);

            return new AuthResult
            {
                Success = true,
                Token =  accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return new AuthResult { Success = false, Message = "Invalid token claims" };

            var user = await _userManager.FindByIdAsync(userId);

            if(user.RefreshToken != refreshToken)
            {
                return new AuthResult { Success = false, Message = "Invalid token claims" };
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Your session has expired. Please login again."
                };
            }

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Something went wrong token"
                };
            }


            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResult
            {
                Success = true,
                Token = _tokenService.CreateToken(user,roles),
                RefreshToken = newRefreshToken
            };  
        }

        public async Task<AuthResult> ConfirmEmailAsync(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return new AuthResult { Success = false, ErrorMessage = "User not found." };
            }
            try
            {
               
                var decodedCodeBytes = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(model.Code);
                var decodedCode = System.Text.Encoding.UTF8.GetString(decodedCodeBytes);
                var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

                if (result.Succeeded)
                {

                    return new AuthResult { Success = true, Message = "Email confirmed successfully!" };
                }

                
                return new AuthResult { Success = false, ErrorMessage = "Invalid or expired token." };
            }
            catch (Exception)
            {
                return new AuthResult { Success = false, ErrorMessage = "Format of the token is invalid." };
            }
        }

        public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new AuthResult { Success = true, Message = "If that email is in our system, you'll receive a link shortly." };
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            byte[] tokenGeneratedBytes = Encoding.UTF8.GetBytes(token);
            var code = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);

            var callbackUrl = $"https://localhost:3000/reset-password?userId={user.Id}&code={code}";


            await _emailService.SendEmailAsync(
                user.Email,
                "Reset Your Password - Astrology App",
                $"Click <a href='{callbackUrl}'>here</a> to reset your password. This link expires in 2 hours."
            );

            return new AuthResult
            {   
                Success=true,
                Message="Reset link sent to your mail."
            };
        }
        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
               
                return new AuthResult { Success = false, Message = "Invalid request." };
            }

            try
            {
            
                var decodedCodeBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedCodeBytes);
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

                if (result.Succeeded)
                {
                    return new AuthResult { Success = true, Message = "Password has been reset successfully!" };
                }

                return new AuthResult
                {
                    Success = false,
                    Errors = result.Errors,
                    Message = "Failed to reset password. The link might be expired."
                };
            }
            catch (Exception)
            {
                return new AuthResult { Success = false, Message = "Invalid token format." };
            }
        }
        public async Task<AuthResult> ChangePasswordAsync(ChangePasswordDto model, string userId) {
            var user =  await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User not found, try again later"
                };
            }
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Failed to update password",
                    Errors = result.Errors
                };
            }

            return new AuthResult
            {
                Success = true,
                Message = "Password updated succesfully"
            };

        }
    }
}
