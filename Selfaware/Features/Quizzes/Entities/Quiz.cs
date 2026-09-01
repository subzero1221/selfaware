
using Selfaware.Features.User.Entities;
using Selfaware.Features.Quizzes.DTOs;

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
        public int TimeLimit { get; set; }

        public QuizStatus QuizStatus { get; set; }

        public QuizType QuizType { get; set; }

        public List<Question> Questions { get; set; } = new();
        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser CreatedBy { get; set; } = null!;
    }
}

public enum QuizStatus
{
    draft = 0,
    approved = 1,
}