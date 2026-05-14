using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Shared.Models;
using System.Security.Claims;
using Selfaware.Features.Auth.DTOs;
using Selfaware.Features.Auth.Entities;
using Selfaware.Features.Quizzes.DTOs;


namespace Selfaware.Features.Auth  
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {


        private readonly IAuthService _authService;
        

        public AuthController(IAuthService authService)
        {
            _authService = authService;
            
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupDto model)
        {
            var platform = Request.Headers["X-Client-Platform"].ToString();
            bool isMobile = platform == "Mobile-App";

            var result = await _authService.SignupUserAsync(model);
            if (!result.Success)
            {

                return BadRequest(new CustomResponse<object>
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = result.Errors.Select(e => e.Description)
                });
               
            }

            if (!isMobile)
            {
                Response.Cookies.Append("refreshToken", result.RefreshToken!, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            return Ok(new CustomResponse<object>

            {
                Success = true,
                Message = "Welcome to astrology"
            });
        }

        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromBody] SigninDto model)
        {
            var platform = Request.Headers["X-Client-Platform"].ToString();
            bool isMobile = platform == "Mobile-App";

            var result = await _authService.SigninAsync(model);

            if (!result.Success)
            {
                return Unauthorized(new CustomResponse<object>
                {
                    Success = false,
                    Message = "Invalid credintials",
                    Errors = new List<string> { result.ErrorMessage }
                });
            }

            if (!isMobile)
            {
                Response.Cookies.Append("refreshToken", result.RefreshToken!, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

           
            return Ok(new CustomResponse<object>
            {
                Success = true,
                Message = "Login successfull",
                Data = result.Token,
            });
        }


        [HttpPost("signout")]
        public IActionResult Signout()
        {

            Response.Cookies.Delete("jwt");
            return Ok(new CustomResponse<object>
            {
                Success = true,
                Message = "Sign out success"
            });
        }

        [Authorize]
        [HttpGet("/me")]
        public async Task<IActionResult> IsAuthorized()
        {
         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         if(userId == null)
            {
                return Unauthorized(new CustomResponse<AuthResult>
                {
                    Message = "Access denied, your not authorized"
                }); 
            }
            return Ok(new CustomResponse<AuthResult>
            {
                Success = true,
                Message = "Acceess granted, authorized user"
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDto model)
        {
            //ვიგებ რექუიესტი მობილურიდანაა თუ ბრაუზერიდან 
            var platform = Request.Headers["X-Client-Platform"].ToString();
            bool isMobile = platform == "Mobile-App";
            string? refreshToken = isMobile? model.RefreshToken : Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new CustomResponse<string>
                {
                    Success = false,
                    Message = "No refresh token provided"
                });
            }

            var accessToken = model.AccessToken;
          
            //ვარეფრეშებ ტოკენს
            var result = await _authService.RefreshTokenAsync(accessToken, refreshToken);
            if (!result.Success)
                return Unauthorized(new CustomResponse<string> { Message = "Refresh failed" });

            //თუ რექვესტი მობილურიდან არაა ვაგზავნი რეფრეშ ტოკენს როგორც ქუქის
            if (!isMobile)
            {
                Response.Cookies.Append("refreshToken", result.RefreshToken!, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            return Ok(new CustomResponse<AuthResult> { Success = true, Data = result });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailDto model)
        {
            if (!ModelState.IsValid)  return BadRequest(new CustomResponse<object>
            {
                Success = false,
                Message = "Invalid parametrs",
                
            });
            
            
            var result = await _authService.ConfirmEmailAsync(model);

            if (result.Success)
            {
                return Ok(new CustomResponse<object>
                {
                    Success = true,
                    Message = "Email confirmed"
                });
            }

            return BadRequest(new CustomResponse<object>
            {
                Success = true,
                Message = "Try again",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new CustomResponse<AuthResult> { Success = false, Message = "Invalid email format" });
            }

            var result = await _authService.ForgotPasswordAsync(model);

            return Ok(new CustomResponse<AuthResult>
            {
                Success = true,
                Message = result.Message,
            });
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new CustomResponse<AuthResult> { Success = false, Message = "Invalid format" });
            }
            var result = await _authService.ResetPasswordAsync(model);

            return Ok(new CustomResponse<AuthResult>
            {
                Success = true,
                Message = result.Message,
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
              return BadRequest(new CustomResponse<AuthResult> { Success = false, Message = "Invalid format" });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            var result = await _authService.ChangePasswordAsync(model, userId);
            return Ok(new CustomResponse<AuthResult>
            {
                Success = result.Success,
                Message = result.Message,
                Errors = result.Errors?.Select(e => e.Description)
            });
        }
    }
}

