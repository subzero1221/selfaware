namespace Selfaware.Features.Quizzes.DTOs
{
    public record ParsedQuestionDto(string Text, List<string> Options, int? CorrectAnswerIndex);
}
