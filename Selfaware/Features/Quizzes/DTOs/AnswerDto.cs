namespace Selfaware.Features.Quizzes.DTOs
{
    public class AnswerDto
    {
        public Guid QuestionId { get; set; }
      
        public string SelectedAnswer { get; set; } = string.Empty;
    }
}
