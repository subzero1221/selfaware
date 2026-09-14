

namespace Selfaware.Features.Survey.Entities
{
    public class Answer
    {
 
       
            public Guid Id { get; set; }
            public Guid? UserId { get; set; }
            
            public Guid QuestionId { get; set; }
            public Guid OptionId { get; set; }

           
            public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        
    }
}

