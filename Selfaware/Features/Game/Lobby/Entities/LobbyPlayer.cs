namespace Selfaware.Features.Game.Lobby.Entities
{
    public class LobbyPlayer
    {
        public Guid PlayerId { get; set; }
 
        public string Nickname { get; set; } = string.Empty;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
