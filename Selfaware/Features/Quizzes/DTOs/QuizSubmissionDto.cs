namespace Selfaware.Features.Quizzes.DTOs
{
    public record QuizSubmissionDto(Guid QuizId, List<AnswerDto> Answers, int TimeTakenMinutes);

    public record AnswerDto(Guid QuestionId, string SelectedAnswer);
}
