using Selfaware.Features.Quizzes.Enums;
using Selfaware.Features.Survey.DTOs;
using System.Text.Json.Serialization;

namespace Selfaware.Features.Survey.SurveySession.Dtos
{

    public record SurveySessionDto(
        Guid Id,
        Guid SurveyId,
        string AnonymousToken,
        DateTime StartedAt,
        bool IsCompleted,
        string Nickname,
        string? Email = null,
        SurveyDto? Survey = null,  
        string? UserId = null,
        DateTime? CompletedAt = null
    );

    public record StartSurveySessionDto(Guid SurveyId, string NickName, string? Email = null);
    public record GetQuestionForSurveySessionDto(string ShareCode, int Order);

    public record UserAnswerDto(Guid Id, Guid SurveySessionId, Guid QuestionId, DateTime SubmittedAt, Guid? OptionId, string? TextAnswer = null, string? UserId = null);
    public record SubmitSurveyAnswerDto(Guid SurveySessionId, Guid QuestionId, Guid OptionId, string? TextAnswer = null, string? UserId = null);

    public record SurveySessionResultDto(IEnumerable<QuestionResultDto> Questions, IList<UserAnswerDto> UserAnswers);
    public record NextQuestionResponseDto(
    QuestionResultDto? Question,
    bool IsCompleted
);

    public record OptionResultDto(
    Guid Id,
    string Text,
    int Score,
    int VoteCount,
    string? ImageUrl = null,
    string? ImagePublicId = null
);

    public record QuestionResultDto(
    Guid Id,
    string Text,
    int Order,
    QuestionType QuestionType,
    int TotalVotes,
    List<OptionResultDto> Options,
    string? ImageUrl = null,
    string? ImagePublicId = null
    );

}
