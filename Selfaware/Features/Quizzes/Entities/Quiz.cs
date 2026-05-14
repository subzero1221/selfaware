namespace Selfaware.Features.Quizzes.Entities
{
    public class Quiz
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
      
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

      
        public List<Question> Questions { get; set; } = new();
    }
}

