namespace Selfaware.Features.Quizzes.DTOs
{
    public class QuizSubmissionDto
    {
        public Guid QuizId { get; set; }
       
        public List<AnswerDto> Answers { get; set; } = new();

       
        public int TimeTakenSeconds { get; set; }
    }
}
