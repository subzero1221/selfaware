

namespace Selfaware.Features.Quizzes.DTOs
{
    public class CreateQuizDto
    {
      
        public string Title { get; set; } = string.Empty;
     
        public string Description { get; set; } = string.Empty;

        public List<CreateQuestionDto> Questions { get; set; } = new();

        public int TimeLimitInMinutes { get; set; }

        public int QuestionsCount { get; set; }
    }
}

