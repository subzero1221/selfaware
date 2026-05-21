using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Auth.DTOs;
using Selfaware.Features.Auth.Entities;
using Selfaware.Shared.Models;
using System.Security.Claims;


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
                return BadRequest(CustomResponse<AuthResult>.ErrorResponse(result.Message, result.Errors));
            }

            return Ok(CustomResponse<AuthResult>.SuccessResponse(result.Data, result.Message));
        }

        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromBody] SigninDto model)
        {
            var platform = Request.Headers["X-Client-Platform"].ToString();
            bool isMobile = platform == "Mobile-App";

            var result = await _authService.SigninAsync(model);

            if (!result.Success)
            {
                return Unauthorized(CustomResponse<AuthResult>.ErrorResponse(result.Message, result.Errors));
            }

            if (!isMobile)
            {
                Response.Cookies.Append("refreshToken", result.Data.RefreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });


                Response.Cookies.Append("accessToken", result.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                });
            }


            return Ok(CustomResponse<AuthResult>.SuccessResponse(result.Data, result.Message));
        }


        [HttpPost("signout")]
        public IActionResult Signout()
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
           return Ok(CustomResponse<AuthResult>.SuccessResponse(null, "signed out succesfully"));
        }

        [Authorize]
        [HttpGet("/me")]
        public async Task<IActionResult> IsAuthorized()
        {
         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         if(userId == null)
            {
                return Unauthorized(CustomResponse<AuthResult>.ErrorResponse("User not found")); 
            }
            return Ok(CustomResponse<AuthResult>.SuccessResponse(null, "signed out succesfully"));
        }

        [HttpGet("refresh")]
        public async Task<IActionResult> Refresh()
        {
            //ვიგებ რექუიესტი მობილურიდანაა თუ ბრაუზერიდან 
            var platform = Request.Headers["X-Client-Platform"].ToString();
            bool isMobile = platform == "Mobile-App"; 
            string? refreshToken =  Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(CustomResponse<AuthResult>.ErrorResponse("Refresh token not provided"));
            }

            
          
            //ვარეფრეშებ ტოკენს
            var result = await _authService.RefreshTokenAsync(refreshToken);
            if (!result.Success)
                return Unauthorized(CustomResponse<string>.ErrorResponse( result.Message, result.Errors));

            //თუ რექვესტი მობილურიდან არაა ვაგზავნი რეფრეშ ტოკენს როგორც ქუქის
            if (!isMobile)
            {
                Response.Cookies.Append("refreshToken", result.Data.RefreshToken!, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                Response.Cookies.Append("accessToken", result.Data.Token!, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                });
            }

            return Ok(CustomResponse<AuthResult>.SuccessResponse(null, "Refresh token success"));
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto model)
        {
            var platform = Request.Headers["X-Client-Platform"].ToString();
            bool isMobile = platform == "Mobile-App";

            var result = await _authService.ConfirmEmailAsync(model);

            if (!result.Success)
            {
                return BadRequest(CustomResponse<AuthResult>.ErrorResponse(result.Message));
            }

            if (!isMobile)
            {
                Response.Cookies.Append("refreshToken", result.Data!.RefreshToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                Response.Cookies.Append("accessToken", result.Data!.Token, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            return Ok(CustomResponse<AuthResult>.SuccessResponse(null, result.Message));
        }

            

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {

            var result = await _authService.ForgotPasswordAsync(model);

            return Ok(CustomResponse<AuthResult>.SuccessResponse(null, result.Message));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
           
            var result = await _authService.ResetPasswordAsync(model);

            if (!result.Success)
            {
                return BadRequest(CustomResponse<AuthResult>.ErrorResponse(result.Message, result.Errors));
            }

            return Ok(CustomResponse<AuthResult>.SuccessResponse(null, result.Message));
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                return Unauthorized(CustomResponse<AuthResult>.ErrorResponse("You dont have access on this action"));
            } 

            var result = await _authService.ChangePasswordAsync(model, userId);
            if (!result.Success)
            {
                return BadRequest(CustomResponse<AuthResult>.ErrorResponse(result.Message, result.Errors));
            }
            return Ok(CustomResponse<AuthResult>.SuccessResponse(null, "Password changed successfully"));
        }
    }
}

