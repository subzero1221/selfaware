namespace Selfaware.Features.Game.GameSession.Entities
{
    public enum SessionState
    {
        Answering = 0,
        ShowingLeaderBoard = 1,
        Finished = 2,
    }

    public class GameSession
    {
        public Guid Id { get; set; }
        public SessionState State { get; set; }

        public string HostId { get; set; } = string.Empty;

        public DateTime StrtedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
