using Selfaware.Features.Survey.DTOs;

namespace Selfaware.Features.Survey.SurveySession.Dtos
{

    public record SurveySessionDto(
        Guid Id,
        Guid SurveyId,
        string AnonymousToken,
        DateTime StartedAt,
        bool IsCompleted,
        string Nickname,
        SurveyDto? Survey = null,  
        string? UserId = null,
        DateTime? CompletedAt = null
    );

    public record StartSurveySessionDto(Guid SurveyId, string NickName);
    public record GetQuestionForSurveySessionDto(string ShareCode, int Order);

    public record UserAnswerDto(Guid Id, Guid SurveySessionId, Guid QuestionId, Guid OptionId, DateTime SubmittedAt, string? UserId = null);
    public record SubmitSurveyAnswerDto(Guid SurveySessionId, Guid QuestionId, Guid OptionId, string? UserId = null);

    public record SurveySessionResultDto(IEnumerable<QuestionResultDto> Questions, IList<UserAnswerDto> UserAnswers);
    public record NextQuestionResponseDto(
    QuestionResultDto? Question,
    bool IsCompleted
);

    public record OptionResultDto(
    Guid Id,
    string Text,
    int VoteCount,
    string? ImageUrl = null,
    string? ImagePublicId = null
);

    public record QuestionResultDto(
        Guid Id,
        string Text,
        int Order,
         int TotalVotes,
        List<OptionResultDto> Options,
        string? ImageUrl = null,
    string? ImagePublicId = null
    );

}
