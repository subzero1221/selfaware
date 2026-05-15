using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.User.DTOs;
using Selfaware.Shared.Models;
using System.Security.Claims;


namespace Selfaware.Features.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController:ControllerBase
    {
        public readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(CustomResponse<UserDto>.ErrorResponse("User ID not found in token" ));
            }

            var result = await _userService.GetMeAsync(userId);
            return Ok(CustomResponse<UserDto>.SuccessResponse(result.Data, result.Message));
    
        }

        [Authorize]
        [HttpPatch("me")]
        public async Task<IActionResult> updateMe([FromBody] UpdateUserDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(CustomResponse<UserDto>.ErrorResponse("User ID not found in token. are u chatlaxob?"));
            }

            var result = await _userService.UpdateMeAsync(userId, dto);


            return Ok(CustomResponse<UserDto>.SuccessResponse(result.Data, result.Message));
        }
    }
}
