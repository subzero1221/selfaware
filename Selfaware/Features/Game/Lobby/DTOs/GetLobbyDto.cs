using Selfaware.Features.Game.Lobby.Entities;

namespace Selfaware.Features.Game.Lobby.DTOs
{
    public record GetLobbyDto(
        Guid Id,
        string HostId,
        string JoinCode,
        LobbyState State,
        IEnumerable<GetLobbyPlayerDto> Players,
        DateTime CreatedAt,
        Guid? QuizId
    );

    public record GetLobbyPlayerDto(
        Guid Id,
        string NickName,
        bool IsReady,
        string SignalRConnectionId,
        DateTime JoinedAt
    );

    public record KickLobbyPlayerDto(string JoinCode, string Id);

    public record JoinLobbyDto(string NickName, string JoinCode);

    public record GetLobbyForPlayerDto(
        Guid Id,
        LobbyState State,
        IEnumerable<GetLobbyPlayerDto> Players
    );
}
