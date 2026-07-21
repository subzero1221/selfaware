using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Features.Game.Lobby.DTOs;
using StackExchange.Redis;
using System.Text.Json;

namespace Selfaware.Features.Game.RedisRepos
{
    public class GamePlayerRedisRepository
    {
        private readonly IDatabase _redis;

        public GamePlayerRedisRepository(IConnectionMultiplexer redisMux)
        {
            _redis = redisMux.GetDatabase();
        }


        private static string PlayersKey(string joinCode) => $"game:{joinCode}:players";

        private static string LobbyPlayersKey(string joinCode) => $"lobby:{joinCode}:players";

        public async Task<List<GetLobbyPlayerDto>> GetLobbyPlayersAsync(string joinCode)
        {
            var entries = await _redis.HashGetAllAsync(LobbyPlayersKey(joinCode));
            if (entries.Length == 0)
                return new List<GetLobbyPlayerDto>(); 

            return entries
                .Select(e => JsonSerializer.Deserialize<GetLobbyPlayerDto>(e.Value.ToString()))
                .OfType<GetLobbyPlayerDto>()
                .ToList();
        }

        public async Task<string> DeleteLobbyPlayerAsync(string joinCode, string playerId)
        {
            await _redis.HashDeleteAsync(LobbyPlayersKey(joinCode), playerId);
            return $"Player with Id: {playerId} deleted";
        }

        public async Task<GetLobbyPlayerDto> GetRequiredLobbyPlayerAsync(string joinCode, string playerId)
        {
            var playerJson = await _redis.HashGetAsync(LobbyPlayersKey(joinCode), playerId);

            if (!playerJson.HasValue)
            {
                throw new KeyNotFoundException($"Player with Id '{playerId}' was not found in lobby '{joinCode}'.");
            }

            return JsonSerializer.Deserialize<GetLobbyPlayerDto>(playerJson.ToString())!;
        }

        public async Task SaveLobbyPlayerAsync(string joinCode, string playerId, GetLobbyPlayerDto player)
        {
           
            string json = JsonSerializer.Serialize(player);
            await _redis.HashSetAsync(LobbyPlayersKey(joinCode), playerId, json);
        }

        public async Task<List<GamePlayerDto>> GetGamePlayersAsync(string joinCode)
        {
            var entries = await _redis.HashGetAllAsync(PlayersKey(joinCode));
            if (entries.Length == 0)
                return new List<GamePlayerDto>();

           var playerList = entries
                .Select(e => JsonSerializer.Deserialize<GamePlayerDto>(e.Value.ToString()))
                .OfType<GamePlayerDto>()
                .ToList();

            if(playerList == null)
            {
                throw new KeyNotFoundException($"Game session with code '{joinCode}' was not found.");
            }

            return playerList;
        }

        public async Task<GamePlayerDto> GetGamePlayerAsync(string joinCode, string playerId)
        {
            RedisValue playerValue = await _redis.HashGetAsync(PlayersKey(joinCode), playerId);
            var player = JsonSerializer.Deserialize<GamePlayerDto>(playerValue.ToString());
            if(player == null)
            {
                throw new KeyNotFoundException($"Player with id '{playerId}' was not found.");
            }

            return player;
        }

        public async Task<string> SetGamePlayerAsync(string joinCode, string playerId, GamePlayerDto updatedPlayer)
        {
            string serializedPlayer = JsonSerializer.Serialize(updatedPlayer);
            await _redis.HashSetAsync(PlayersKey(joinCode), playerId, serializedPlayer);
            await _redis.KeyExpireAsync(PlayersKey(joinCode), TimeSpan.FromHours(1));
            return "player set";
        }

        public async Task<string> SetPlayersAsync(string joinCode, List<GamePlayerDto> gamePlayerList)
        {
            var playerHashEntries = gamePlayerList
                .Select(player => new HashEntry(
                    player.PlayerId.ToString(),
                    JsonSerializer.Serialize(player)
                ))
                .ToArray();
        
            await _redis.HashSetAsync(PlayersKey(joinCode), playerHashEntries);
            return "done";
        }


    }
}