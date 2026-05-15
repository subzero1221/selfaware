using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Selfaware.Features.Auth.DTOs;
using Selfaware.Features.Auth.Entities;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.User.Entities;
using Selfaware.Infrastructure.Messaging;
using Selfaware.Shared.Models;
using System.Security.Claims;
using System.Text;



namespace Selfaware.Features.Auth
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

        public async Task<ServiceResult<AuthResult>> SignupUserAsync(SignupDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.Email,
            };

            string password = model.Password;

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description);
                return ServiceResult<AuthResult>.Failed("User creation failed", errorMessages);
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

            var data = new AuthResult
            {
                Token = accessToken,
                RefreshToken = refreshToken
            };

            return ServiceResult<AuthResult>.Ok(data, "Registration successful! Please check your email to confirm your account.");
           
        }

        public async Task <ServiceResult<AuthResult>> SigninAsync(SigninDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return ServiceResult<AuthResult>.Failed("Email or Password incorrect!");
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return ServiceResult<AuthResult>.Failed("Email or Password incorrect!");


            var refreshToken = _tokenService.GenerateRefreshToken();

         
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.CreateToken(user, roles);

            var data = new AuthResult
            {
                Token =  accessToken,
                RefreshToken = refreshToken
            };

            return ServiceResult<AuthResult>.Ok(data, "Singed in Successfully");

        }

        public async Task<ServiceResult<AuthResult>> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return ServiceResult<AuthResult>.Failed("Invalid Token claims!");

            var user = await _userManager.FindByIdAsync(userId);

            if(user.RefreshToken != refreshToken)
            {
                return ServiceResult<AuthResult>.Failed("Invalid Token claims!");
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return ServiceResult<AuthResult>.Failed("Your session expired, Please try to log in!");
            }

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description);
                return ServiceResult<AuthResult>.Failed("Something went wrong", errorMessages);
            }


            var roles = await _userManager.GetRolesAsync(user);

            var data = new AuthResult
            {
                Token = accessToken,
                RefreshToken = refreshToken
            };

            return ServiceResult<AuthResult>.Ok(data, "refreshtoken updated succesfully");
        }


        public async Task<ServiceResult<AuthResult>> ConfirmEmailAsync(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return ServiceResult<AuthResult>.Failed("User not found.");
            }
          
                var decodedCodeBytes = WebEncoders.Base64UrlDecode(model.Code);
                var decodedCode = Encoding.UTF8.GetString(decodedCodeBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedCode);
                if (!result.Succeeded)
                {

                    return ServiceResult<AuthResult>.Failed("Email confirmation failed", result.Errors.Select(e => e.Description));
                }
              return ServiceResult<AuthResult>.Ok(null , "Email confirmed Succesfully");

        }

            
        

        public async Task<ServiceResult<AuthResult>> ForgotPasswordAsync(ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return ServiceResult<AuthResult>.Ok(null, "If that email is in our system, you'll receive a link shortly."); 
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

            return ServiceResult<AuthResult>.Ok(null, "Password reset link sent to your email.");
        }

        public async Task<ServiceResult<AuthResult>> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return ServiceResult<AuthResult>.Failed("User not found.");
            }

           
            
                var decodedCodeBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedCodeBytes);
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

                if (!result.Succeeded)
                {
                    var errorMessages = result.Errors.Select(e => e.Description);
                    return ServiceResult<AuthResult>.Failed("Could not reset password.", errorMessages);
                }

              
              return ServiceResult<AuthResult>.Ok(null,"Password reset succesfully");
            
        }
        public async Task<ServiceResult<AuthResult>> ChangePasswordAsync(ChangePasswordDto model, string userId) {
            var user =  await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<AuthResult>.Failed("User not found.");
            }
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description);
                return ServiceResult<AuthResult>.Failed("Could not change password.", errorMessages);
            }

            return ServiceResult<AuthResult>.Ok(null, "Password changed succesfully");

        }
    }
}
