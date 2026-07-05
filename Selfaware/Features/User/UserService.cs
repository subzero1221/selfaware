using Microsoft.AspNetCore.Identity;
using Selfaware.Features.User.DTOs;
using Selfaware.Features.User.Entities;
using Selfaware.Shared.Models;

namespace Selfaware.Features.User
{
    public class UserService : IUserService
    {
        public readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ServiceResult<UserDto>> GetMeAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return ServiceResult<UserDto>.Failed("User Not found");

            var userDto = new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                Email = user.Email,
            };

            return ServiceResult<UserDto>.Ok(userDto, "Get me success");
        }

        public async Task<ServiceResult<UserDto>> UpdateMeAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return ServiceResult<UserDto>.Failed("User not found.");

            if (dto.DisplayName != null)
                user.DisplayName = dto.DisplayName;
            if (dto.Bio != null)
                user.Bio = dto.Bio;
            if (dto.Email != null)
                user.Email = dto.Email;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description);
                return ServiceResult<UserDto>.Failed("Update failed.", errorMessages);
            }

            var updatedUserDto = new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                Email = user.Email,
            };

            return ServiceResult<UserDto>.Ok(updatedUserDto, "User updated successfully");
        }

        public async Task<ServiceResult<UserDto>> DeleteMeAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResult<UserDto>.Failed("User not found");

            var userDto = new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
            };

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description);
                return ServiceResult<UserDto>.Failed(
                    "Failed to deactivate account.",
                    errorMessages
                );
            }

            return ServiceResult<UserDto>.Ok(
                userDto,
                "Your account has been deactivated successfully"
            );
        }
    }
}
