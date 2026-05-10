namespace Selfaware.Features.Quizzes.DTOs
{
    public class CreateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public string OptionsJson { get; set; } = "[]";
    }
}
