using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Features.Game.GameSession.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace Selfaware.Features.Game.RedisRepos
{
    public class GameRedisRepository
    {
        private readonly IDatabase _redis;

        public GameRedisRepository(IConnectionMultiplexer redisMux)
        {
            _redis = redisMux.GetDatabase();
        }

        private static string GameKey(string joinCode) => $"game:{joinCode}";

        private static string LobbyKey(string joinCode) => $"lobby:{joinCode}";

        private static string LockKey(string joinCode)=> $"game:{joinCode}:lock:nextquestion";

        public async Task<bool> LockGameAsync(string joinCode)
        {
            bool acquiredLock = await _redis.StringSetAsync(
                LockKey(joinCode),
                "locked",
                TimeSpan.FromSeconds(3),
                When.NotExists
            );

            return acquiredLock;
        }
        public async Task<Dictionary<string, string>> GetGameMetaAsync(string joinCode)
        {
            var entries = await _redis.HashGetAllAsync(GameKey(joinCode));

            if (entries.Length == 0)
            {
                throw new KeyNotFoundException($"Game session with code '{joinCode}' was not found.");
            }


            return entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());

        }

        public async Task<Dictionary<string, string>> GetLobbyAsync(string joinCode)
        {
            var entries = await _redis.HashGetAllAsync(LobbyKey(joinCode));
            if (entries.Length == 0)
            {
                throw new KeyNotFoundException($"Game session with code '{joinCode}' was not found.");
            }
            return entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
        }

        public async Task StartGameSessionAsync(
            string joinCode,
            Guid gameId,
            Guid quizId,
            string hostId,
            int totalQuestions,
            ActiveQuestionDto initialQuestion,
            Guid correctOptionId,
            long expiresAtUnix
        )
        {
            var gameData = new HashEntry[]
            {
                new HashEntry("Id", gameId.ToString()),
                new HashEntry("QuizId", quizId.ToString()),
                new HashEntry("HostId", hostId),
                new HashEntry("State", SessionState.Answering.ToString()),
                new HashEntry("CurrentQuestionIndex", "0"),
                new HashEntry("TotalQuestionCount", totalQuestions.ToString()),
                new HashEntry("CurrentQuestionId", initialQuestion.Id.ToString()),
                new HashEntry("CorrectOptionId", correctOptionId.ToString()),
                new HashEntry("CurrentQuestion", JsonSerializer.Serialize(initialQuestion)),
                new HashEntry("QuestionTimeExpiresAt", expiresAtUnix.ToString()),
            };

            await _redis.HashSetAsync(GameKey(joinCode), gameData);
            await _redis.KeyExpireAsync(GameKey(joinCode), TimeSpan.FromHours(1));
           
        }

        public async Task SaveGameMetaAsync(string joinCode, IEnumerable<HashEntry> updates)
        {
            await _redis.HashSetAsync(GameKey(joinCode), updates.ToArray());
        }

        public async Task<RedisValue[]> GetMetaFieldsAsync(string joinCode, params string[] fields)
        {
            var redisFields = fields.Select(f => (RedisValue)f).ToArray();
            return await _redis.HashGetAsync(GameKey(joinCode), redisFields);
        }
    }
}
