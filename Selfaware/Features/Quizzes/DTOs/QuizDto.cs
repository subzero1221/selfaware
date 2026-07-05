using System.Text.Json.Serialization;
using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.DTOs
{
    public record QuizDto(
        Guid QuizId,
        string Description,
        int QuestionCount,
        int TimeLimitInMinutes,
        string? Title = null
    );

    public record AiQuizResponseDto(List<AiQuestionDto> Questions);

    public record AiQuestionDto(string Text, List<AiOptionDto> Options);

    public record AiOptionDto(string Text, int Score);
}
