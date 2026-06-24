using Selfaware.Shared.Models;

namespace Selfaware.Features.Game.GameSession
{
    public interface IGameSessionService
    {
        public Task<ServiceResult<string>> PlayerIsReadyAsync(string playerId, string joinCode, string ConnectionId);
        public Task<ServiceResult<string>> PlayerIsNotReadyAsync(string playerId, string joinCode, string ConnectionId);
        public Task<ServiceResult<string>> StartGameAsync(string joinCode, string hostId, Guid quizId, string connectionId);
    }
}
