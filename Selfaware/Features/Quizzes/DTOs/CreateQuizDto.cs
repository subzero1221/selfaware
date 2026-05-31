using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.DTOs
{

    public record CreateQuizDto(string Title, string Description, List<CreateQuestionDto> Questions, int TimeLimitInMinutes, int QuestionCount); 
    public record CreateQuestionDto(string Text, List<OptionDto> Options, int Order, QuestionType QuestionType);
    public record BulkQuizUpload(int TimeLimitInMinutes, IFormFile File, string? Title = null, string? Description = null);

    public record ExtractQuizRequestDto(IFormFile File);
    
}

