using Microsoft.Identity.Client;
using Selfaware.Features.Quizzes.DTOs;

namespace Selfaware.Features.Survey.DTOs
{
    public record SurveyDto
    (
        Guid SurveyId,
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
        List<QuestionDto> Questions,
        int QuestionCount,
        string? Description = null,
        string? Title = null
    );

    //enums
    
}
