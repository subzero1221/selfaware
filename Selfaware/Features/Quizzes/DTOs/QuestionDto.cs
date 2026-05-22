namespace Selfaware.Features.Quizzes.DTOs
{
    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string OptionsJson { get; set; } = string.Empty;
    }
}
