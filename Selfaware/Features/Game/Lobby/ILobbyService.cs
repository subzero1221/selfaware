using Selfaware.Features.Game.Lobby.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Game.Lobby
{
    public interface ILobbyService
    {
        Task<ServiceResult<GetLobbyDto>> GetLobbyAsync(string hostId);
        Task<ServiceResult<string>> CreateLobbyAsync(string hostId);

        Task<ServiceResult<string>> DeleteLobbyAsync(string hostId);
        Task<ServiceResult<string>> KickLobbyPlayerAsync(KickLobbyPlayerDto dto);

        Task<ServiceResult<GetLobbyPlayerDto>> JoinLobbyAsync(JoinLobbyDto dto);

        Task<ServiceResult<GetLobbyForPlayerDto>> GetLobbyForPlayerAsync(
            string joinCode,
            Guid playerId
        );
    }
}
