using Selfaware.Features.Game.GameSession.Entities;


namespace Selfaware.Features.Game.GameSession.DTOs
{
    public record GameDto(
        Guid Id, 
        Guid QuizId, 
        ActiveQuestionDto CurrentQuestion, 
        int CurrentQuestionIndex, 
        ICollection<GamePlayerDto> Players, 
        SessionState State, int TotalQuestions, 
        int? TimeLimitSeconds
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
}
