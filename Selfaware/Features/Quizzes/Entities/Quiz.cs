using Selfaware.Features.Quizzes.Enums;
using Selfaware.Features.User.Entities;

namespace Selfaware.Features.Quizzes.Entities
{
    public class Quiz
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int QuestionCount { get; set; }
        public TimeSpan TimeLimit { get; set; }

        public QuizStatus QuizStatus { get; set; }

    
        public string QuizType { get; set; } = "Knowledge";

        public List<Question> Questions { get; set; } = new();
        public string CreatedById { get; set; } = string.Empty; 
        public ApplicationUser CreatedBy { get; set; } = null!; 
    }
}

