namespace Selfaware.Features.Game.GameSession.Entities
{
    public enum SessionState
    {
        Answering = 0,
        ShowingLeaderBoard = 1,
        Finished = 2,
    }

    public class GameSessionEntity
    {
        public Guid Id { get; set; }
        public SessionState State { get; set; }

        public Guid HostId { get; set; }

        public DateTime StrtedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Player> Players { get; set; } = new List<Player>();
    }

    public enum PlayerState
    {
        Answering = 0,
        Answered = 1,
        Disconnected = 2,
    }

    public class Player
    {
        public Guid PlayerId { get; set; }
        public string NickName { get; set; } = string.Empty;

        public int Score { get; set; }

    }
}
