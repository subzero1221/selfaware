using System.ComponentModel.DataAnnotations.Schema;

namespace Selfaware.Models.Entities
{
    public class UserSubmission
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid QuizId { get; set; }

        [Column(TypeName = "jsonb")]
        public string RawAnswersJson { get; set; } = "[]";

        public bool IsPaid { get; set; } = false;
        public string? AiReport { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
