using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Game.GameSession
{
    public interface IGameSessionService
    {
        //gamelogic
        public Task<ServiceResult<GameDto>> StartGameAsync(
            string joinCode,
            string hostId,
            Guid quizId,
            string connectionId
        );

        public Task<ServiceResult<GameDto>> ShowLeaderBoardAsync(string joinCode, string playerId);
        public Task<ServiceResult<GameDto>> NextQuestionAsync(string joinCode, string playerId);

        //Player staff
        public Task<ServiceResult<string>> LeaveLobbyAsync(string playerId, string joinCode);

        public Task<ServiceResult<string>> PlayerIsReadyAsync(
            string playerId,
            string joinCode,
            string ConnectionId
        );
        public Task<ServiceResult<string>> PlayerIsNotReadyAsync(
            string playerId,
            string joinCode,
            string ConnectionId
        );
        public Task<ServiceResult<GamePlayerDto>> SubmitAnswerAsync(SubmitAnswerDto dto);

        //HttpReqs
        public Task<ServiceResult<GameDto>> GetGameAsync(string joinCode, string playerId);
    }
}
