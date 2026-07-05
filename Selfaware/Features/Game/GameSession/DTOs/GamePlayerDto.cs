using Selfaware.Features.Game.GameSession.Entities;

public enum PlayerState
{
    Answering = 0,
    Answered = 1,
    Disconnected = 2,
}

namespace Selfaware.Features.Game.GameSession.DTOs
{
    public record GamePlayerDto(
        Guid PlayerId,
        string NickName,
        PlayerState State,
        int Score,
        int Streak,
        string SignalRConnectionId
    );
}
