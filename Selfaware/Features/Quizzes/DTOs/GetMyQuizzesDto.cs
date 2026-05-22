namespace Selfaware.Features.Quizzes.DTOs
{
    public class GetMyQuizzesDto
    {
        public List<QuizSummaryDto> Quizzes { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
