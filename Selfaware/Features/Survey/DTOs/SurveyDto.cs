
using Selfaware.Features.Quizzes.DTOs;

namespace Selfaware.Features.Survey.DTOs
{
    public record SurveyDto
    (
        Guid Id,
        string RunById,
        QuizForSurveyDto Quiz,
        string ShareCode,
        int CompletedBy,
        bool IsActive,
        bool AllowAnonymous,
        DateTime? ExpiresAt,
        DateTime CreatedAt,
        DateTime LastActivatedAt
    
        );


    public class ActivateSurveyDto
    {
        public Guid QuizId { get; set; }

        public int? DurationInDays { get; set; }

        public bool AllowAnonymous { get; set; } = true;
    }

    public record QuizForSurveyDto
    (
        Guid QuizId,
        QuizType QuizType,
        QuizStatus QuizStatus,
        int QuestionCount,
        List<QuestionDto>? Questions = null,
        string? Description = null,
        string? Title = null
    );

    //enums
    
}
