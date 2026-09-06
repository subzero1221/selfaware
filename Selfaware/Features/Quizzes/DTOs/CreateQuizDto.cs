using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.DTOs
{
    public record CreateQuizDto(
        string Title,
        string Description,
        List<CreateQuestionDto> Questions,
        int TimeLimitInMinutes,
        int QuestionCount, 
        QuizType QuizType
    );

    public record CreateQuestionDto(
        string Text,
        List<OptionDto> Options,
        int Order,
        QuestionType QuestionType,
        string? ImageUrl = null,
        string? ImagePublicId = null
    );

    public record PutQuizDto(
        Guid Id,
        string Title,
        string Description,
        string Slug,
        int TimeLimit,
        int QuestionCount,
        QuizStatus QuizStatus,
        QuizType QuizType,
        List<QuestionDto> Questions,
        string? ImageUrl = null,
        string? ImagePublicId = null
    );

    public record BulkQuizUpload(
        int TimeLimitInMinutes,
        IFormFile File,
        string? Title = null,
        string? Description = null
    );

    public class ExtractQuizRequestDto
    {
        public IFormFile? File { get; set; }
    };

    //enums
    public enum QuizType
    {
        Knowledge = 0,
        Survey = 1,
    }
}
