

namespace Selfaware.Features.Game.Lobby.Entities
{
    public enum LobbyState
    {
        WaitingForPlayers,
        Active,
        Finished           
    }

    public class Lobby
    {
        public Guid Id { get; set; }
        public string JoinCode { get; set; } = string.Empty;

        public Guid QuizId { get; set; }
   

        public string HostId { get; set; }

        public LobbyState State { get; set; } = LobbyState.WaitingForPlayers;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LobbyPlayer> Players { get; set; } = new List<LobbyPlayer>();
    }
}
