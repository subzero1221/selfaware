namespace Selfaware.Features.Survey.SurveySession.Dtos
{
    public record SurveySessionDto(Guid Id, Guid SurveyId, string AnonymousToken, DateTime StartedAt, bool IsCompleted,  string? Nickname = null,string? UserId = null, DateTime? CompletedAt = null);

    public record StartSurveySessionDto(Guid SurveyId, string? NickName = null);
    public record GetQuestionForSurveySessionDto(Guid SurveyId, int Order);
    
}
