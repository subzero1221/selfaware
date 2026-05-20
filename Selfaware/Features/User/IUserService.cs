using Selfaware.Features.User.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Features.User
{
    public interface IUserService
    {
        Task<ServiceResult<UserDto>> GetMeAsync(string userId);
        Task<ServiceResult<UserDto>> UpdateMeAsync(string userId, UpdateUserDto dto);

        Task<ServiceResult<UserDto>> DeleteMeAsync(string userId);
    }
}
