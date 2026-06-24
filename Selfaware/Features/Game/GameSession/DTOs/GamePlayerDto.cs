using Selfaware.Features.Game.GameSession.Entities;

public enum PlayerState
{
    Waiting = 0,
    Answering = 1,
    Answered = 2,
    Disconnected = 3
}

namespace Selfaware.Features.Game.GameSession.DTOs
{
    public record GamePlayerDto(Guid PlayerId, string NickName, PlayerState State, int Score, int Streak, string SignalRConnectionId);
    
       
    
}
