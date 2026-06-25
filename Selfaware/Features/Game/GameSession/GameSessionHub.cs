using Microsoft.AspNetCore.SignalR;
using Selfaware.Features.Game.GameSession.Entities;

namespace Selfaware.Features.Game.GameSession
{
    public class GameSessionHub:Hub
    {
        private readonly IGameSessionService _gameSessionService;
        public GameSessionHub(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        public async Task JoinLobby(string joinCode)
        {
      
            await Groups.AddToGroupAsync(Context.ConnectionId, joinCode);

        }

        public async Task LeaveLobby(string playerId, string joinCode)
        {
            string connectionId = Context.ConnectionId;
            await _gameSessionService.LeaveLobbyAsync(playerId, joinCode);
            await Groups.RemoveFromGroupAsync(joinCode, connectionId);

            await Clients.Client(connectionId).SendAsync("LeaveLobby");
        }

        public async Task PlayerIsReady(string playerId, string joinCode)
        {

            string connectionId = Context.ConnectionId;
            await _gameSessionService.PlayerIsReadyAsync(playerId, joinCode, connectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, joinCode);
       
            await Clients.Group(joinCode).SendAsync("PlayerIsReady", playerId);
        }



        public async Task PlayerIsNotReady(string playerId, string joinCode)
        {

            string connectionId = Context.ConnectionId;
            await _gameSessionService.PlayerIsNotReadyAsync(playerId, joinCode, connectionId);
            await Clients.Group(joinCode).SendAsync("PlayerIsNotReady", playerId);
        }

        public async Task StartGame(string joinCode, string hostId, Guid quizId)
        {
            string connectionId = Context.ConnectionId;
            var result = await _gameSessionService.StartGameAsync(joinCode, hostId, quizId, connectionId);
            if (!result.Success)
            {
                await Clients.Caller.SendAsync("StartGameFail", result.Message);
                return;
            }

           await Clients.Group(joinCode).SendAsync("StartGame", result.Data);
        }
    }
}
