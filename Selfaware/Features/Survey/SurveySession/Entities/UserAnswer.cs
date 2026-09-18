using Selfaware.Features.Quizzes.Entities;

namespace Selfaware.Features.Survey.SurveySession.Entities
{
    public class UserAnswer
    {
            public Guid Id { get; set; }
            public Guid? UserId { get; set; }
            public Guid QuestionId { get; set; }
            
            public Guid SurveySessionId { get; set; }

        public Guid OptionId { get; set; }
            public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public SurveySession SurveySession { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public Option Option { get; set; } = null!;
    }
}
