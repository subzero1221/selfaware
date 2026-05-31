using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Features.Quizzes.Entities
{
    public class Question
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }

        public Quiz Quiz { get; set; }

        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }

    
        public QuestionType Type { get; set; } = QuestionType.SingleChoice;

     
        public List<Option> Options { get; set; } = new();
    }

    public class Option
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Score { get; set; }
    }
}