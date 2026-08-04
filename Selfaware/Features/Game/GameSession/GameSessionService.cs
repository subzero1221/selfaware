using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Features.Game.GameSession.Entities;
using Selfaware.Features.Game.GameSession.Helpers;
using Selfaware.Features.Game.RedisRepos;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Selfaware.Features.Game.GameSession
{
    public class GameSessionService : IGameSessionService
    {

        private readonly AppDbContext _context;
        private readonly GameRedisRepository _gameRepo;
        private readonly GamePlayerRedisRepository _gamePlayerRepo;

        public GameSessionService(
            AppDbContext context,
            GameRedisRepository gameRepo,
            GamePlayerRedisRepository gamePlayerRepo
        )
        {
            _context = context;
            _gameRepo = gameRepo;
            _gamePlayerRepo = gamePlayerRepo;
        }

        public async Task<ServiceResult<string>> PlayerIsReadyAsync(
            string playerId,
            string joinCode,
            string ConnectionId
        )
        {
            var player = await _gamePlayerRepo.GetRequiredLobbyPlayerAsync(joinCode, playerId);

            
            var updatedPlayer = player with { IsReady = true };

        
            await _gamePlayerRepo.SaveLobbyPlayerAsync(joinCode, playerId, updatedPlayer);

            return ServiceResult<string>.Ok("Player readiness updated successfully.");
        }
        

        public async Task<ServiceResult<string>> PlayerIsNotReadyAsync(
            string playerId,
            string joinCode,
            string ConnectionId
        )
        {
            var player = await _gamePlayerRepo.GetRequiredLobbyPlayerAsync(joinCode, playerId);


            var updatedPlayer = player with { IsReady = false};


            await _gamePlayerRepo.SaveLobbyPlayerAsync(joinCode, playerId, updatedPlayer);

            return ServiceResult<string>.Ok("Player readiness updated successfully.");
        }

        public async Task<ServiceResult<string>> LeaveLobbyAsync(string playerId, string joinCode)
        {
            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(joinCode))
            {
                return ServiceResult<string>.Failed("Invalid ID or Code");
            }

            await _gamePlayerRepo.DeleteLobbyPlayerAsync(joinCode, playerId);

            return ServiceResult<string>.Ok(playerId, "Player deleted succesfully");
        }

        public async Task<ServiceResult<GameDto>> StartGameAsync(
            string joinCode,
            string hostId,
            Guid quizId,
            string connectionId
        )
        {
            //Getting players from lobby
            var playersList = await _gamePlayerRepo.GetLobbyPlayersAsync(joinCode);

            //Declaring new props for game
            Guid gameId = Guid.NewGuid();
            string startedAtIso = DateTime.UtcNow.ToString("o");
            int TotalQuestionCount = await _context
                .Questions.Where(q => q.QuizId == quizId)
                .CountAsync();

            var question = await _context
                .Questions.Include(q => q.Options)
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.Order)
                .ThenBy(question => question.Id)
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return ServiceResult<GameDto>.Failed("This quiz doesnt have any questions");
            }

            var correctOption = question.Options.FirstOrDefault(option => option.Score == 1);
            if (correctOption == null)
            {
                return ServiceResult<GameDto>.Failed("Active question is missing a designated correct option.");
            }

            Guid correctOptionId = correctOption.Id;

            var activeQuestionDto = new ActiveQuestionDto(
                Id: question.Id,
                Text: question.Text,
                Options: question
                    .Options.Select(option => new ActiveOptionDto(option.Id, option.Text))
                    .ToList()
            );
            
            long expiresAtUnix = DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeSeconds();

            //setting new started game into hash
            await _gameRepo.StartGameSessionAsync(
                joinCode,
                gameId,
                quizId,
                hostId,
                TotalQuestionCount,
                activeQuestionDto,
                correctOptionId,
                expiresAtUnix
            );


            var gamePlayerList = playersList
                .Where(player => player.IsReady)
                .Select(player => new GamePlayerDto(
                    PlayerId: player.Id,
                    NickName: player.NickName,
                    State: 0,
                    Score: 0,
                    Streak: 0,
                    SignalRConnectionId: player.SignalRConnectionId
                ))
                .ToList();

            //setting gameplayers into hash
            await _gamePlayerRepo.SetPlayersAsync(joinCode, gamePlayerList);

            var newGame = new GameDto(
                Id: gameId,
                QuizId: quizId,
                CurrentQuestion: new ActiveQuestionDto(
                    Id: question.Id,
                    Text: question.Text,
                    Options: question
                        .Options.Select(option => new ActiveOptionDto(
                            Id: option.Id,
                            Text: option.Text
                        ))
                        .ToList(),
                QuestionImageUrl: question.ImageUrl
                        ),
                
                CurrentQuestionIndex: 0,
                Players: gamePlayerList,
                State: SessionState.Answering,
                TotalQuestions: TotalQuestionCount,
                TimeLimitSeconds: 10,
                 TimeLeft: 10
            );

            Console.WriteLine($"Enter Game Service{newGame} quizid");

            return ServiceResult<GameDto>.Ok(newGame, "Game is Ready");
        }

        public async Task<ServiceResult<GameDto>> ShowLeaderBoardAsync(
            string joinCode,
            string playerId
        )
        {

            var playerList = await _gamePlayerRepo.GetGamePlayersAsync(joinCode);
            bool isPlayerInGame = playerList.Any(player => player.PlayerId.ToString() == playerId);
            if (!isPlayerInGame)
            {
                return ServiceResult<GameDto>.Failed("Your not in the game");
            }


            var gameMeta = await _gameRepo.GetGameMetaAsync(joinCode);


            Guid gameId = Guid.Parse(gameMeta["Id"]);
            Guid quizId = Guid.Parse(gameMeta["QuizId"]);
            int currentIndex = int.Parse(gameMeta["CurrentQuestionIndex"]);
            long expiresAtUnix = DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeSeconds();

            var updatedGame = new HashEntry[]
            {
                new HashEntry("State", SessionState.ShowingLeaderBoard.ToString()),
                new HashEntry("leaderBoardTimeLeft", expiresAtUnix),
            };

            await _gameRepo.SaveGameMetaAsync(joinCode, updatedGame);

            var game = new GameDto(
                Id: gameId,
                QuizId: quizId,
                CurrentQuestionIndex: currentIndex,
                Players: playerList,
                State: SessionState.ShowingLeaderBoard,
                 TotalQuestions: null,
                TimeLimitSeconds: 10,
                 TimeLeft: 10
            );

            return ServiceResult<GameDto>.Ok(game, "Leaderboard return success");
        }

        public async Task<ServiceResult<GameDto>> NextQuestionAsync(
            string joinCode,
            string playerId
        )
        {

            bool acquiredLock = await _gameRepo.LockGameAsync(joinCode);
            if (!acquiredLock)
            {
                return await GetGameAsync(joinCode, playerId);
            }

            var playerList = await _gamePlayerRepo.GetGamePlayersAsync(joinCode);

            var updatedPlayerList = playerList
                .Select(p => p with { State = PlayerState.Answering })
                .ToList();

            await _gamePlayerRepo.SetPlayersAsync(joinCode, updatedPlayerList);


            var gameMeta = await _gameRepo.GetGameMetaAsync(joinCode);
           

            Guid gameId = Guid.Parse(gameMeta["Id"]);
            Guid quizId = Guid.Parse(gameMeta["QuizId"]);
            int currentIndex = int.Parse(gameMeta["CurrentQuestionIndex"]);
            int totalQuestionCount = int.Parse(gameMeta["TotalQuestionCount"]);

            int nextIndex = currentIndex + 1;

            if(nextIndex == totalQuestionCount)
            {
                Console.WriteLine($"Now i will finish the game---------------");
                return await FinishGameAsync(joinCode);   
            }

            var question = await _context
                .Questions.Include(question => question.Options)
                .Where(question => question.QuizId == quizId)
                .OrderBy(question => question.Order)
                .ThenBy(question => question.Id)
                .Skip(nextIndex)
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return ServiceResult<GameDto>.Failed("Active question not found");
            }

            

            var activeQuestionDto = new ActiveQuestionDto(
                Id: question.Id,
                Text: question.Text,
                Options: question
                    .Options.Select(option => new ActiveOptionDto(option.Id, option.Text))
                    .ToList(),
                QuestionImageUrl:question.ImageUrl
            );

            var correctOption = question.Options.FirstOrDefault(option => option.Score == 1);
            long expiresAtUnix = DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeSeconds();

            var gameUpdates = new HashEntry[]
            {
                new HashEntry("State", SessionState.Answering.ToString()),
                new HashEntry("CurrentQuestionIndex", nextIndex.ToString()),
                new HashEntry("CurrentQuestionId", question.Id.ToString()),
                new HashEntry("CorrectOptionId", correctOption?.Id.ToString() ?? string.Empty),
                new HashEntry("CurrentQuestion", JsonSerializer.Serialize(activeQuestionDto)),
                new HashEntry("QuestionTimeExpiresAt", expiresAtUnix.ToString()),
            };
            await _gameRepo.SaveGameMetaAsync(joinCode, gameUpdates);

            var updatedGame = new GameDto(
                Id: gameId,
                QuizId: quizId,
                CurrentQuestion: new ActiveQuestionDto(
                    Id: question.Id,
                    Text: question.Text,
                    Options: question
                        .Options.Select(option => new ActiveOptionDto(
                            Id: option.Id,
                            Text: option.Text
                        ))
                        .ToList(),
                     QuestionImageUrl: question.ImageUrl
                ),
                CurrentQuestionIndex: nextIndex,
                Players: updatedPlayerList,
                State: SessionState.Answering,
                TotalQuestions: totalQuestionCount,
                TimeLimitSeconds: 10,
                TimeLeft: 10
            );

            return ServiceResult<GameDto>.Ok(updatedGame, "Next question set successfully");
        }

        public async Task<ServiceResult<GamePlayerDto>> SubmitAnswerAsync(SubmitAnswerDto dto)
        {

            var player = await _gamePlayerRepo.GetGamePlayerAsync(dto.JoinCode, dto.PlayerId);

            var validate = new[] { "CurrentQuestionId", "CorrectOptionId", "QuestionTimeExpiresAt" };
            var validationRules = await _gameRepo.GetMetaFieldsAsync(dto.JoinCode, validate);


            string currentQuestionId = validationRules[0];
            Console.WriteLine(
                $"Questionid vs currquestionid: {dto.QuestionId} vs {currentQuestionId}"
            );
            string correctOptionId = validationRules[1];
            long expiresAtUnix = long.Parse(validationRules[2]);
            long currentTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long difference = expiresAtUnix - currentTimeUnix;
            int timeLeftSeconds = Math.Max(0, (int)difference);

            if (currentQuestionId != dto.QuestionId)
            {
                return ServiceResult<GamePlayerDto>.Failed("This question is no longer active.");
            }

            bool playerIsCorrect = dto.OptionId == correctOptionId;
            Console.WriteLine($"{playerIsCorrect} cuz: {dto.OptionId} and {correctOptionId}");
            GamePlayerDto updatedPlayer;

            if (!playerIsCorrect)
            {
                updatedPlayer = new GamePlayerDto(
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
                int newScore = GameCalculator.CalculateScore(
                    timeLeftSeconds,
                    player.Score,
                    player.Streak+1
                );
                updatedPlayer = new GamePlayerDto(
                    PlayerId: player.PlayerId,
                    NickName: player.NickName,
                    State: PlayerState.Answered,
                    Score: newScore,
                    Streak: player.Streak + 1,
                    SignalRConnectionId: player.SignalRConnectionId
                );
            }

            await _gamePlayerRepo.SetGamePlayerAsync(dto.JoinCode, dto.PlayerId, updatedPlayer);

            return ServiceResult<GamePlayerDto>.Ok(updatedPlayer, "Asnwer submit success");
        }


         public async Task<ServiceResult<GameDto>> FinishGameAsync(string joinCode)
         {
             var gameMeta = await _gameRepo.GetGameMetaAsync(joinCode);
             var players = await _gamePlayerRepo.GetGamePlayersAsync(joinCode);
             Guid gameId = Guid.Parse(gameMeta["Id"]);
             Guid quizId = Guid.Parse(gameMeta["QuizId"]);
             Guid hostId = Guid.Parse(gameMeta["HostId"]);
             DateTime startedAt = DateTime.Parse(gameMeta["StartedAt"]);

            var gameUpdates = new HashEntry[]
            {
               new HashEntry("State", SessionState.Finished.ToString()),
            };
            await _gameRepo.SaveGameMetaAsync(joinCode, gameUpdates);


            var finishGameRes = new GameDto(
                            Id: gameId,
                            QuizId: quizId,
                            Players: players,
                            State: SessionState.Finished
                        );

            return ServiceResult<GameDto>.Ok(finishGameRes, "Game finished successfully");
             

         }
        

        //Http Reqs
        public async Task<ServiceResult<GameDto>> GetGameAsync(string joinCode, string playerId)
        {

            var gameMeta = await _gameRepo.GetGameMetaAsync(joinCode);
            var playerList = await _gamePlayerRepo.GetGamePlayersAsync(joinCode);



            Guid gameId = Guid.Parse(gameMeta["Id"]);
            Guid quizId = Guid.Parse(gameMeta["QuizId"]);
            int currentIndex = int.Parse(gameMeta["CurrentQuestionIndex"]);
            int totalQuestionCount = int.Parse(gameMeta["TotalQuestionCount"]);
            string stateString = gameMeta["State"];

            long expiresAtUnix =
                stateString == "Answering"
                    ? long.Parse(gameMeta["QuestionTimeExpiresAt"])
                    : long.Parse(gameMeta["leaderBoardTimeLeft"]);

            long currentTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long difference = expiresAtUnix - currentTimeUnix;
            int timeLeftSeconds = Math.Max(0, (int)difference);

            SessionState sessionState = Enum.Parse<SessionState>(stateString);

            string questionJson = gameMeta["CurrentQuestion"];
            var activeQuestion = JsonSerializer.Deserialize<ActiveQuestionDto>(questionJson);

            if (activeQuestion == null)
            {
                return ServiceResult<GameDto>.Failed("Active question not found");
            }

            var newGame = new GameDto(
                Id: gameId,
                QuizId: quizId,
                CurrentQuestion: new ActiveQuestionDto(
                    Id: activeQuestion.Id,
                    Text: activeQuestion.Text,
                    Options: activeQuestion
                        .Options.Select(option => new ActiveOptionDto(
                            Id: option.Id,
                            Text: option.Text
                        ))
                        .ToList(),
                     QuestionImageUrl: activeQuestion.QuestionImageUrl
                ),
                CurrentQuestionIndex: currentIndex,
                Players: playerList,
                State: sessionState,
                TotalQuestions: totalQuestionCount,
                TimeLimitSeconds: 10,
                TimeLeft: timeLeftSeconds
            );

            return ServiceResult<GameDto>.Ok(newGame, "Game found successfully");
        }

        public async Task<ServiceResult<GameDto>> GetGameForHostAsync(string joinCode)
        {

            var gameMeta = await _gameRepo.GetGameMetaAsync(joinCode);
            var playerList = await _gamePlayerRepo.GetGamePlayersAsync(joinCode);



            Guid gameId = Guid.Parse(gameMeta["Id"]);
            Guid quizId = Guid.Parse(gameMeta["QuizId"]);
            int currentIndex = int.Parse(gameMeta["CurrentQuestionIndex"]);
            int totalQuestionCount = int.Parse(gameMeta["TotalQuestionCount"]);
            string stateString = gameMeta["State"];

            long expiresAtUnix =
                stateString == "Answering"
                    ? long.Parse(gameMeta["QuestionTimeExpiresAt"])
                    : long.Parse(gameMeta["leaderBoardTimeLeft"]);

            long currentTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long difference = expiresAtUnix - currentTimeUnix;
            int timeLeftSeconds = Math.Max(0, (int)difference);

            SessionState sessionState = Enum.Parse<SessionState>(stateString);

            string questionJson = gameMeta["CurrentQuestion"];
            var activeQuestion = JsonSerializer.Deserialize<ActiveQuestionDto>(questionJson);

            if (activeQuestion == null)
            {
                return ServiceResult<GameDto>.Failed("Active question not found");
            }

            var newGame = new GameDto(
                Id: gameId,
                QuizId: quizId,
                CurrentQuestion: new ActiveQuestionDto(
                    Id: activeQuestion.Id,
                    Text: activeQuestion.Text,
                    Options: activeQuestion
                        .Options.Select(option => new ActiveOptionDto(
                            Id: option.Id,
                            Text: option.Text
                        ))
                        .ToList(),
                     QuestionImageUrl: activeQuestion.QuestionImageUrl
                ),
                CurrentQuestionIndex: currentIndex,
                Players: playerList,
                State: sessionState,
                TotalQuestions: totalQuestionCount,
                TimeLimitSeconds: 10,
                TimeLeft: timeLeftSeconds
            );

            return ServiceResult<GameDto>.Ok(newGame, "Game found successfully");
        }

        public async Task<ServiceResult<string>> SaveFinishedGameAsync(string joinCode, string userId)
        {

            var gameMeta = await _gameRepo.GetGameMetaAsync(joinCode);
            if (gameMeta == null || !gameMeta.Any())
            {
                return ServiceResult<string>.Failed("Game session not found or has expired.");
            }


            if (!gameMeta.TryGetValue("HostId", out var hostId) || hostId != userId)
            {
                return ServiceResult<string>.Failed("You are not allowed to perform this action.");
            }


            var players = await _gamePlayerRepo.GetGamePlayersAsync(joinCode);


            Guid gameId = Guid.Parse(gameMeta["Id"]);
            Guid quizId = Guid.Parse(gameMeta["QuizId"]);
            Guid hostGuid = Guid.Parse(hostId);
            DateTime startedAt = DateTime.Parse(gameMeta["StartedAt"]).ToUniversalTime();


            bool isAlreadySaved = await _context.GameSessionEntities.AnyAsync(g => g.Id == gameId);
            if (isAlreadySaved)
            {
                return ServiceResult<string>.Ok("თამაში უკვე შენახულია.");
            }

            var savedGame = new GameSessionEntity
            {
                Id = gameId,
                HostId = hostGuid,
                StrtedAt = startedAt,
                State = SessionState.Finished,
                Players = players.Select(player => new Player
                {
                    PlayerId = player.PlayerId,
                    NickName = player.NickName,
                    Score = player.Score,
                }).ToList()
            };



         
            _context.GameSessionEntities.Add(savedGame);
            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok("Game saved successfully.");
        }


    }
}
