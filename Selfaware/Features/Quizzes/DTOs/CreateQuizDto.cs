using System.ComponentModel.DataAnnotations;

namespace Selfaware.Features.Quizzes.DTOs
{
    public class CreateQuizDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Title cannot be empty")]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters long")]
        public string Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        public List<CreateQuestionDto> Questions { get; set; } = new();
    }
}

