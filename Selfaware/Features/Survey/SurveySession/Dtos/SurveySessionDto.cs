namespace Selfaware.Features.Survey.SurveySession.Dtos
{
    public record SurveySessionDto(Guid Id, Guid SurveyId, string AnonymousToken, DateTime StartedAt, bool IsCompleted,  string? Nickname = null,string? UserId = null, DateTime? CompletedAt = null);

    public record StartSurveySessionDto(string ShareCode, string? NickName = null);
    public record GetQuestionForSurveySessionDto(string ShareCode, int Order);

    public record UserAnswerDto(Guid Id, Guid QuestionId, Guid OptionId, DateTime SubmitedAt, string? UserId = null);
    public record SubmitSurveyAnswerDto(Guid QuestionId, Guid OptionId, string? UserId = null);
}
