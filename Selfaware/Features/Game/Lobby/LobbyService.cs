using Selfaware.Features.Game.Lobby.DTOs;
using Selfaware.Features.Game.Lobby.Entities;
using Selfaware.Shared.Models;
using StackExchange.Redis;
using System.Text.Json;



namespace Selfaware.Features.Game.Lobby
{
    public class LobbyService : ILobbyService
    {
        private readonly IDatabase _redis;

        public LobbyService(IConnectionMultiplexer redisMux)
        {
            _redis = redisMux.GetDatabase();
        }

        public async Task<ServiceResult<GetLobbyDto>> GetLobbyAsync(string hostId)
        {
            RedisValue joinCode = await _redis.HashGetAsync("lookup:host:lobby", hostId);
            if (!joinCode.HasValue)
            {
                return ServiceResult<GetLobbyDto>.Failed("No active lobby found for this host.");
            }
            string joinCodeStr = $"lobby:{joinCode}";
            var lobbyTask = _redis.HashGetAllAsync(joinCodeStr);
            var playersTask = _redis.HashGetAllAsync($"{joinCodeStr}:players");
           
            //var currentDb = _redis.Database;
          

            await Task.WhenAll(lobbyTask, playersTask);
            var hashEntries = lobbyTask.Result;
            var playerEntries = playersTask.Result;

            if (hashEntries.Length == 0)
            {
                return ServiceResult<GetLobbyDto>.Failed("Lobby has expired or does not exist.");
            }

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


            var lobbyDto = new GetLobbyDto(
               Id: Guid.Parse(lobbyDict["Id"]),
               HostId: hostId,
               QuizId: string.IsNullOrEmpty(lobbyDict["QuizId"]) ? null : Guid.Parse(lobbyDict["QuizId"]),
               JoinCode: joinCode.ToString(),
               State: Enum.Parse<LobbyState>(lobbyDict["State"]),
               Players: playersList,
               CreatedAt: DateTime.UtcNow
    );

            return ServiceResult<GetLobbyDto>.Ok(lobbyDto, "Lobby retrieved successfully");
        }

        public async Task<ServiceResult<string>> CreateLobbyAsync(string hostId)
        {
            string joinCode = await GenerateUniquePinAsync();
            string lobbyKey = $"lobby:{joinCode}";
            Guid lobbyId = Guid.NewGuid();
            string createdAtIso = DateTime.UtcNow.ToString("o");


            var lobbyData = new HashEntry[]
         {
        new HashEntry("Id", lobbyId.ToString()),
        new HashEntry("QuizId", string.Empty),
        new HashEntry("HostId", hostId),
        new HashEntry("State", "WaitingForPlayers"),
        new HashEntry("CurrentQuestionIndex", "0"),
        new HashEntry("CreatedAt", createdAtIso)
         };

            var transaction = _redis.CreateTransaction();

     
            transaction.AddCondition(Condition.KeyNotExists(lobbyKey));


            _ = transaction.HashSetAsync(lobbyKey, lobbyData);
            _ = transaction.KeyExpireAsync(lobbyKey, TimeSpan.FromHours(2));
            _ = transaction.HashSetAsync("lookup:host:lobby", hostId, joinCode);

            bool success = await transaction.ExecuteAsync();

            if (!success)
            {
           
                
                return ServiceResult<string>.Failed("Redis transaction failed to commit. Try creating the room again.");
            }

            return ServiceResult<string>.Ok(joinCode, "Lobby created succesfully");
        }

        public async Task<ServiceResult<string>> DeleteLobbyAsync(string hostId)
        {
            RedisValue joinCode = await _redis.HashGetAsync("lookup:host:lobby", hostId);
            if (!joinCode.HasValue)
            {
                return ServiceResult<string>.Failed("No active lobby found for this host.");
            }

            string joinCodeStr = joinCode.ToString();
            string lobbyKey = $"lobby:{joinCodeStr}";
            string playersKey = $"lobby:{joinCodeStr}:players";

            
            await _redis.HashDeleteAsync("lookup:host:lobby", hostId);

            await _redis.KeyDeleteAsync(lobbyKey);
            await _redis.KeyDeleteAsync(playersKey);

     

            return ServiceResult<string>.Ok(joinCodeStr, "Lobby deleted successfully");

        }


        public async Task<ServiceResult<string>> KickLobbyPlayerAsync(KickLobbyPlayerDto dto)
        {
            string joinCode = dto.JoinCode;
            string playerId = dto.Id;

            var playersKey = $"lobby:{joinCode}:players";
           

            await _redis.HashDeleteAsync(playersKey, playerId.ToString());

            return ServiceResult<string>.Ok(playerId, "Player deleted succesfully");

        }


        //Players staff
        public async Task<ServiceResult<GetLobbyPlayerDto>> JoinLobbyAsync(JoinLobbyDto dto)
        {
            string joinCode = dto.JoinCode;
            string nickName = dto.NickName;

            string playersKey = $"lobby:{joinCode}:players";
            string lobbyKey = $"lobby:{joinCode}";

            bool lobbyExists = await _redis.KeyExistsAsync(lobbyKey);
            if (!lobbyExists)
            {
                return ServiceResult<GetLobbyPlayerDto>.Failed("Lobby not found. Double check your PIN!");
            }

            if (string.IsNullOrWhiteSpace(nickName)) 
            {
                return ServiceResult<GetLobbyPlayerDto>.Failed("Nickname is not provided");
            }

            var stateValue = await _redis.HashGetAsync(lobbyKey, "State");
            if (stateValue.HasValue && stateValue.ToString() != LobbyState.WaitingForPlayers.ToString())
            {
                return ServiceResult<GetLobbyPlayerDto>.Failed("The game has already started!");
            }

            Guid playerId = Guid.NewGuid();
            DateTime joinedAt = DateTime.UtcNow;

     
            var playerPayload = new
            {
                Id = playerId,
                NickName = nickName,
                IsReady = false,
                JoinedAt = joinedAt.ToString("o")
            };

            string playerJson = JsonSerializer.Serialize(playerPayload);

          
            await _redis.HashSetAsync(playersKey, playerId.ToString(), playerJson);

          
            var newPlayer = new GetLobbyPlayerDto(
                Id: playerId,
                NickName: nickName,
                IsReady:false,
                SignalRConnectionId:"",
                JoinedAt: joinedAt
            );

            return ServiceResult<GetLobbyPlayerDto>.Ok(newPlayer, "You joined lobby successfully");
        }

        public async Task<ServiceResult<GetLobbyForPlayerDto>> GetLobbyForPlayerAsync(string joinCode, Guid playerId)
        {
            string joinCodeStr = $"lobby:{joinCode}";
            string checkPlayer = $"{joinCodeStr}:{playerId}";
          

            var lobbyTask = _redis.HashGetAllAsync(joinCodeStr);
            var playersTask = _redis.HashGetAllAsync($"{joinCodeStr}:players");

            var currentDb = _redis.Database;


            await Task.WhenAll(lobbyTask, playersTask);
            var hashEntries = lobbyTask.Result;
            var playerEntries = playersTask.Result;

            bool isPlayerInLobby = playerEntries.Any(entry => entry.Name.ToString() == playerId.ToString());

            if (!isPlayerInLobby)
            {
                return ServiceResult<GetLobbyForPlayerDto>.Failed("Lobby not found");
            }
        

            if (hashEntries.Length == 0)
            {
                return ServiceResult<GetLobbyForPlayerDto>.Failed("Lobby has expired or does not exist.");
            }


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



            var lobby = new GetLobbyForPlayerDto(
              Id: Guid.Parse(lobbyDict["Id"]),
              State: Enum.Parse<LobbyState>(lobbyDict["State"]),
              Players: playersList
              );

            return ServiceResult<GetLobbyForPlayerDto>.Ok(lobby, "Lobby found succesfully");
        }

        private async Task<string> GenerateUniquePinAsync()
        {
            while (true)
            {

                string pin = Random.Shared.Next(100000, 1000000).ToString();
                string lobbyKey = $"lobby:{pin}";

                bool exists = await _redis.KeyExistsAsync(lobbyKey);

                if (!exists)
                {
                    return pin;
                }
            }
        }

    }
}
