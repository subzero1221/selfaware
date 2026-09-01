using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.DTOs
{
    namespace Selfaware.Features.Quizzes.DTOs
    {
        public record GetQuizzesDto(int TotalCount, List<QuizSummaryDto>? Quizzes = null);

        public record QuizDetailsDto(
            Guid Id,
            int TimeLimit,
            QuizStatus QuizStatus,
            QuizType QuizType,
            string? Title = null,
            string? Description = null,
            string? Slug = null,
            List<QuestionDto>? Questions = null
        );

        public record QuizSummaryDto(
            Guid Id,
            int QuestionCount,
            QuizStatus QuizStatus,
            QuizType QuizType,
            string? Title = null,
            string? Slug = null,
            string? Description = null,
            string? CreatedById = null
        );
    }
}
