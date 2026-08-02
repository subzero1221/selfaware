using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.DTOs
{
    public record EditQuizSettingsDto(SettingsField Field, string Value);

    public record EditQuestionDto(Guid Id, string Text, EditOptionDto[] Options, string? ImageUrl = null, string? ImagePublicId=null);

    public record EditOptionDto(Guid Id, string Text, int Score);
}
