namespace Selfaware.Features.Quizzes.DTOs
{
    public class QuizSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public string QuizType { get; set; } = string.Empty;

        public string createdById { get; set; } = string.Empty;
    }
}
