namespace Selfaware.Features.Quizzes.Entities
{
    public class Question
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public Guid QuizId { get; set; }
        public string? QuestionType { get; set; }

        public string OptionsJson { get; set; } = "[]";
    }
}
