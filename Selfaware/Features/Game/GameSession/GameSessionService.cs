
using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Features.Game.GameSession.Entities;
using Selfaware.Features.Game.GameSession.Helpers;
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

                return ServiceResult<string>.Failed("Invalid ID or Code");
            }
            string playersKey = $"lobby:{joinCode}:players";
            await _redis.HashDeleteAsync(playersKey, playerId);

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


            string questionKey = $"game:{joinCode}";
            var correctOption = question.Options
                               .FirstOrDefault(option => option.Score == 1);


            var questionForRedis = new HashEntry[]
                {
                new HashEntry("QuestionId", question.Id.ToString()),
                new HashEntry("OptionId", correctOption!.Id.ToString())
                };


            await _redis.HashSetAsync(questionKey, questionForRedis);


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

        public async Task<ServiceResult<GamePlayerDto>> SubmitAnswerAsync(SubmitAnswerDto dto)
        {
            string gameKey = $"game:{dto.JoinCode}";
            var gameEntries = await _redis.HashGetAllAsync(gameKey);
            if (gameEntries.Length == 0)
            {
                return ServiceResult<GamePlayerDto>.Failed("Game not found.");
            }

            string playersKey = $"game:{dto.JoinCode}:players";
            RedisValue playerValue = await _redis.HashGetAsync(playersKey, dto.PlayerId);

            if (!playerValue.HasValue)
            {
                return ServiceResult<GamePlayerDto>.Failed("You are not a member of this lobby");
            }

            var player = JsonSerializer.Deserialize<GamePlayerDto>(playerValue.ToString());
            if (player == null)
            {
                return ServiceResult<GamePlayerDto>.Failed("Player not found");
            }

            if (player.State == PlayerState.Answered)
            {
                return ServiceResult<GamePlayerDto>.Failed("You have already submitted an answer for this question.");
            }


            var validationRules = await _redis.HashGetAsync(gameKey, new RedisValue[] { "CurrentQuestionId", "CorrectOptionId" });
            

            string currentQuestionId = validationRules[0];
            string correctOptionId = validationRules[1];

            if (currentQuestionId != dto.QuestionId)
            {
                return ServiceResult<GamePlayerDto>.Failed("This question is no longer active.");
            }

            bool playerIsCorrect = dto.OptionId == correctOptionId;
            GamePlayerDto updatedPlayer;

            if (!playerIsCorrect)
            {
                updatedPlayer = new GamePlayerDto
                (
                     PlayerId: player.PlayerId,
                     NickName: player.NickName,
                     State: PlayerState.Answered,
                     Score: player.Score,
                     Streak: 0,
                     SignalRConnectionId: player.SignalRConnectionId
                );
            }
            else
            {
                int newScore = GameCalculator.CalculateScore(dto.OnSecond, player.Score, player.Streak);
                updatedPlayer = new GamePlayerDto
                (
                     PlayerId: player.PlayerId,
                     NickName: player.NickName,
                     State: PlayerState.Answered,
                     Score: newScore,
                     Streak: player.Streak + 1,
                     SignalRConnectionId: player.SignalRConnectionId
                );
            }

            string serializedPlayer = JsonSerializer.Serialize(updatedPlayer);
            await _redis.HashSetAsync(playersKey, dto.PlayerId, serializedPlayer);

            return ServiceResult<GamePlayerDto>.Ok(updatedPlayer, "Asnwer submit success");

        }


        //Http Reqs
        public async Task<ServiceResult<GameDto>> GetGameAsync(string joinCode, string playerId)
        {
            string gameKey = $"game:{joinCode}";
            var gameEntries = await _redis.HashGetAllAsync(gameKey);
            if (gameEntries.Length == 0)
            {
                return ServiceResult<GameDto>.Failed("Game not found.");
            }

            string playersKey = $"game:{joinCode}:players";
            var playerEntries = await _redis.HashGetAllAsync(playersKey);

            bool isPlayerInGame = playerEntries.Any(player => player.Name == playerId.ToString());
            if (!isPlayerInGame)
            {
                return ServiceResult<GameDto>.Failed("You are not member of this lobby");
            }

            var playerList = playerEntries
           .Select(p => JsonSerializer.Deserialize<GamePlayerDto>(p.Value.ToString()))
           .OfType<GamePlayerDto>()
           .ToList();

            var gameMeta = gameEntries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());

            Guid gameId = Guid.Parse(gameMeta["Id"]);
            Guid quizId = Guid.Parse(gameMeta["QuizId"]);
            int currentIndex = int.Parse(gameMeta["CurrentQuestionIndex"]);
            string stateString = gameMeta["State"];

            SessionState sessionState = Enum.Parse<SessionState>(stateString);

            var question = await _context.Questions
                           .Include(question => question.Options)
                           .Where(question => question.QuizId == quizId)
                           .OrderBy(question => question.Order)
                           .Skip(currentIndex)
                           .FirstOrDefaultAsync();

            if (question == null)
            {
                return ServiceResult<GameDto>.Failed("Active question not found");
            }

            int totalQuestionCount = await _context.Questions.CountAsync(question => question.QuizId == quizId);

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
                Players: playerList,
                State: SessionState.Answering,
                TotalQuestions: totalQuestionCount,
                TimeLimitSeconds: 30
                );

            return ServiceResult<GameDto>.Ok(newGame, "Game found successfully");

        }

    }
}
