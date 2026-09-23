

namespace Selfaware.Features.Survey.SurveySession.Entities
{
    public class SurveySession
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public Survey.Entities.Survey Survey { get; set; } = null!;
        public ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
        public string? UserId { get; set; }
        public string AnonymousToken { get; set; } = string.Empty;

        public string? Nickname { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
