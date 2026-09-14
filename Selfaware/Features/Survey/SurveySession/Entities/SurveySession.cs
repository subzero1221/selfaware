namespace Selfaware.Features.Survey.SurveySession.Entities
{
    public class SurveySession
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public string? UserId { get; set; }
        public string AnonymousToken { get; set; } = string.Empty;

        public string? Nickname { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
