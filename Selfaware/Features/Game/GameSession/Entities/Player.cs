namespace Selfaware.Features.Game.GameSession.Entities
{

    public enum PlayerState
    {
        Waiting = 0,
        Answering = 1,
        Answered = 2,
        Disconnected = 3
    }

    public class Player
    {
        public Guid PlayerId { get; set; }
        public string NickName { get; set; } = string.Empty;

        public PlayerState State { get; set; }
        public int Score { get; set; }
        public int Streak { get; set; } = 0;

        public string SignalRConnectionId { get; set; }
    }
}
