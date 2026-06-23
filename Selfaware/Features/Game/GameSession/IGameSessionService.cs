using Selfaware.Shared.Models;

namespace Selfaware.Features.Game.GameSession
{
    public interface IGameSessionService
    {
        public Task<ServiceResult<string>> StartGameAsync(string joinCode, string hostId, Guid quizId, string connectionId);
    }
}
