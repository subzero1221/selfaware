using Microsoft.AspNetCore.SignalR;

namespace Selfaware.Features.Game.GameSession
{
    public class GameSessionHub:Hub
    {
        private readonly IGameSessionService _gameSessionService;
        public GameSessionHub(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        public async Task StartGame(string joinCode, string hostId, Guid quizId)
        {
            string connectionId = Context.ConnectionId;
            await _gameSessionService.StartGameAsync(joinCode, hostId, quizId, connectionId);

        }

    }
}
