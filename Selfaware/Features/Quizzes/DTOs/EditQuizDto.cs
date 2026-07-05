using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.DTOs
{
    public record EditQuizSettingsDto(SettingsField Field, string Value);

    public record EditQuestionDto(Guid Id, string Text, EditOptionDto[] Options);

    public record EditOptionDto(Guid Id, string Text, int Score);
}
