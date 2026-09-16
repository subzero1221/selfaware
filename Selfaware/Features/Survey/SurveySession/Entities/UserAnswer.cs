namespace Selfaware.Features.Survey.SurveySession.Entities
{
    public class UserAnswer
    {
            public Guid Id { get; set; }
            public Guid? UserId { get; set; }
            public Guid QuestionId { get; set; }
            public Guid OptionId { get; set; }
            public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
