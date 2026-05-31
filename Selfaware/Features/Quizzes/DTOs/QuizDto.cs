
using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.DTOs
{
 
    public record QuizDto(Guid QuizId, string Description, int QuestionCount, int TimeLimitInMinutes, string? Title = null);
    public record AiQuizResponseDto(List<AiQuestionDto> Questions);
    public record AiQuestionDto(List<AiOptionDto> Options, string? Text = null);
    public record AiOptionDto(int Score, string? Text = null);

}
