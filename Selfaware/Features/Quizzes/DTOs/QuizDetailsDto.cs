namespace Selfaware.Features.Quizzes.DTOs
{
    public class QuizDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan TimeLimit { get; set; }

        
        public List<QuestionDto> Questions { get; set; } = new();
    }
}
