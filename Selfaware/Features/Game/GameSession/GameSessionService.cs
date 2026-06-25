
using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Features.Game.GameSession.Entities;
using Selfaware.Features.Game.Lobby.DTOs;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.Models;
using StackExchange.Redis;
using System.Text.Json;


namespace Selfaware.Features.Game.GameSession
{
    public class GameSessionService : IGameSessionService
    {
        private readonly IDatabase _redis;
        private readonly AppDbContext _context;

        public GameSessionService(IConnectionMultiplexer redisMux, AppDbContext context)
        {
            _redis = redisMux.GetDatabase();
            _context = context;
        }

        public async Task<ServiceResult<string>> PlayerIsReadyAsync(string playerId, string joinCode, string ConnectionId)
        {

            string playersKey = $"lobby:{joinCode}:players";
            string playerJson = await _redis.HashGetAsync(playersKey, playerId);

            if (string.IsNullOrEmpty(playerJson))
            {
                return ServiceResult<string>.Failed("Player not found.");
            }

            var player = JsonSerializer.Deserialize<GetLobbyPlayerDto>(playerJson);
            var updatedPlayer = player with { IsReady = true, SignalRConnectionId = ConnectionId };
            string updatedJson = JsonSerializer.Serialize(updatedPlayer);
            await _redis.HashSetAsync(playersKey, playerId, updatedJson);

            return ServiceResult<string>.Ok(updatedPlayer.SignalRConnectionId, "Player is ready!");


        }

        public async Task<ServiceResult<string>> PlayerIsNotReadyAsync(string playerId, string joinCode, string ConnectionId)
        {

            string playersKey = $"lobby:{joinCode}:players";


            string playerJson = await _redis.HashGetAsync(playersKey, playerId);

            if (string.IsNullOrEmpty(playerJson))
            {
                return ServiceResult<string>.Failed("Player not found.");
            }

            var player = JsonSerializer.Deserialize<GetLobbyPlayerDto>(playerJson);

            var updatedPlayer = player with { IsReady = false };
            string updatedJson = JsonSerializer.Serialize(updatedPlayer);

            await _redis.HashSetAsync(playersKey, playerId, updatedJson);

            return ServiceResult<string>.Ok(updatedPlayer.SignalRConnectionId, "Player is ready!");
        }

        public async Task<ServiceResult<string>> LeaveLobbyAsync(string playerId, string joinCode)
        {

            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(joinCode))
            {
                Console.WriteLine($"PlayerID:{playerId} and joincode: {joinCode}");
                return ServiceResult<string>.Failed("Invalid ID or Code");
            }
            string playersKey = $"lobby:{joinCode}:players";
            Console.WriteLine("imHEREEE");
            await _redis.HashDeleteAsync(playersKey, playerId);
            Console.WriteLine("imHEREEE222222222");
            return ServiceResult<string>.Ok(playerId, "Player deleted succesfully");

        }

        public async Task<ServiceResult<GameDto>> StartGameAsync(string joinCode, string hostId, Guid quizId, string connectionId)
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

            await _redis.HashSetAsync(gameKey, gameData);

           

            var gamePlayerList = playersList.Select(player => new GamePlayerDto(
                PlayerId: player.Id,
                NickName: player.NickName,
                State: 0,
                Score: 0,
                Streak: 0,
                SignalRConnectionId: player.SignalRConnectionId
                )).ToList();

            var playerHashEntries = gamePlayerList.Select(player =>
            new HashEntry(player.PlayerId.ToString(), JsonSerializer.Serialize(player))
             ).ToArray();

            await _redis.HashSetAsync(gamePlayersKey, playerHashEntries);

            //Console.WriteLine($"Enter Game Service{quizId} quizid");

            var question = await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.Order)
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return ServiceResult<GameDto>.Failed("This quiz doesnt have any questions");
            }

            int TotalQuestionCount = await _context.Questions.Where(q => q.QuizId == quizId).CountAsync();
          
            var newGame = new GameDto(
                Id: gameId,
                QuizId: quizId,
                CurrentQuestion: new ActiveQuestionDto
                (
                 Id: question.Id,
                 Text: question.Text,
                 Options: question.Options.Select(option =>
                 new ActiveOptionDto
                 (
                     Id: option.Id,
                     Text: option.Text
                     )).ToList()
                    ),
                CurrentQuestionIndex: 0,
                Players: gamePlayerList,
                State: SessionState.Answering,
                TotalQuestions: TotalQuestionCount,
                TimeLimitSeconds: 30
                );

            Console.WriteLine($"Enter Game Service{newGame} quizid");

            return ServiceResult<GameDto>.Ok(newGame, "Game is Ready");
        }

    }
}
