using Selfaware.Features.Game.GameSession.Entities;


namespace Selfaware.Features.Game.GameSession.DTOs
{
    public record GameDto(
        Guid Id,
        Guid QuizId,
        int CurrentQuestionIndex,
        ICollection<GamePlayerDto> Players,
        SessionState State,
        ActiveQuestionDto? CurrentQuestion = null,
        int? TotalQuestions = null,
        int? TimeLimitSeconds = null
        );


    public record ActiveQuestionDto(
        Guid Id,
        string Text,
        ICollection<ActiveOptionDto> Options
    );

    public record ActiveOptionDto(
        Guid Id,
        string Text
    );

    public record GetGameDto(string joinCode, string playerId);

    public record SubmitAnswerDto(
    string JoinCode,
    string PlayerId,
    string QuestionId,
    int OnSecond,
    string OptionId
);



    //for game logic
    public record LeaderBoardDto(IList<GamePlayerDto> Players);
    public record NextQuestionDto(string JoinCode, int CurrentQuestionIndex);
    public record correctQuestionDto(Guid QuestionId, Guid CorrectOptionId);
}
