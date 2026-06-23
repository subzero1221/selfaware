
using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Features.Game.GameSession.Entities;
using Selfaware.Features.Game.Lobby.DTOs;
using Selfaware.Features.Game.Lobby.Entities;
using Selfaware.Shared.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Selfaware.Features.Game.GameSession
{
    public class GameSessionService : IGameSessionService
    {
        private readonly IDatabase _redis;

        public GameSessionService(IConnectionMultiplexer redisMux)
        {
            _redis = redisMux.GetDatabase();
        }

        public async Task<ServiceResult<string>> StartGameAsync(string joinCode, string hostId, Guid quizId, string connectionId)
        {
            string joinCodeStr = $"lobby:{joinCode}";
            var lobbyTask = _redis.HashGetAllAsync(joinCodeStr);
            var playersTask = _redis.HashGetAllAsync($"{joinCodeStr}:players");

            await Task.WhenAll(lobbyTask, playersTask);
            var hashEntries = lobbyTask.Result;
            var playerEntries = playersTask.Result;

            var lobbyDict = hashEntries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());


            var playersList = new List<GetLobbyPlayerDto>();
            foreach (var player in playerEntries)
            {
                var playerDto = JsonSerializer.Deserialize<GetLobbyPlayerDto>(player.Value.ToString());
                if (playerDto != null)
                {
                    playersList.Add(playerDto);
                }
            }

            string gameKey = $"game:{joinCode}";
            string gamePlayersKey = $"game:{joinCode}:players";
            Guid gameId = Guid.NewGuid();
            string startedAtIso = DateTime.UtcNow.ToString("o");

            var gameData = new HashEntry[]
         {
                 new HashEntry("Id", gameId.ToString()),
                 new HashEntry("QuizId", quizId.ToString()),
                 new HashEntry("HostId", hostId),
                 new HashEntry("State", SessionState.Answering.ToString()),
                 new HashEntry("CurrentQuestionIndex", 0),
                 new HashEntry("CreatedAt", startedAtIso)
         };

            var gamePlayerList = playersList.Select(player => new GamePlayerDto(
                PlayerId: player.Id,
                NickName: player.NickName,
                State: 0,
                Score: 0,
                Streak: 0,
                SignalRConnectionId: connectionId

                )).ToList();

            await _redis.HashSetAsync(gameKey, gameData);
            foreach(var player in gamePlayerList)
            {
                var playerJson = JsonSerializer.Serialize(player);
                await _redis.HashSetAsync(gamePlayersKey, player.PlayerId.ToString(), playerJson);
            }

        }

    }
}
