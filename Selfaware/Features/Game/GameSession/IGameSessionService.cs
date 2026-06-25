using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Game.GameSession
{
    public interface IGameSessionService
    {
        public Task<ServiceResult<string>> PlayerIsReadyAsync(string playerId, string joinCode, string ConnectionId);
        public Task<ServiceResult<string>> PlayerIsNotReadyAsync(string playerId, string joinCode, string ConnectionId);
        public Task<ServiceResult<GameDto>> StartGameAsync(string joinCode, string hostId, Guid quizId, string connectionId);

        public Task<ServiceResult<string>> LeaveLobbyAsync(string playerId, string joinCode);
    }
}
