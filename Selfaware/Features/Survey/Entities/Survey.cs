namespace Selfaware.Features.Survey.Entities
{
    public class Survey
    {
       public Guid Id { get; set; }
        
       public Guid QuizId { get; set; }
       public string ShareCode { get; set; } = string.Empty;
       
        public int CompletedBy { get; set; }
       public bool IsActive { get; set; }
       public bool AllowAnonymous { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
       public DateTime LastActivatedAt { get; set; } = DateTime.UtcNow;
    }
}
