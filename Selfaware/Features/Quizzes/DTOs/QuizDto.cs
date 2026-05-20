namespace Selfaware.Features.Quizzes.DTOs
{
    public class QuizDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; }
        public int QuestionCount { get; set; }
        public int TimeLimitInMinutes { get; set; }
    }
}
