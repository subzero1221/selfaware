using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.DTOs
{
    public record QuestionDto(Guid Id, string Text, QuestionType Type, int Order, List<OptionDto> Options);
    
    public record ParsedQuestionDto(string Text, List<OptionDto> Options);

    public record OptionDto(Guid Id, string Text, int Score);
}
