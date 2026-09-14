namespace Selfaware.Features.Survey.DTOs
{
    public record SubmitAnswerDto(Guid QuestionId, Guid OptionId);

    //public record AnsweredQuestionResultDto(Guid QuestionId, Guid Option);
    
}
