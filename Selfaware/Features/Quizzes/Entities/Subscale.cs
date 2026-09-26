namespace Selfaware.Features.Quizzes.Entities
{
    public class Subscale
    {
       public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<Question> Questions { get; set; } = new();

    }
}
