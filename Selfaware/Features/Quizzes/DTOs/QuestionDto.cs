using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.DTOs
{
    public record QuestionDto(
        Guid Id,
        string Text,
        QuestionType Type,
        int Order,
        List<OptionDto> Options,
        string? ImageUrl = null,
        string? ImagePublicId = null
    );

    public record ParsedQuestionDto(string Text, List<OptionDto> Options);

    public record OptionDto(Guid Id, string Text, int Score);
}
