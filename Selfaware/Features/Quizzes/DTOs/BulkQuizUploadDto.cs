namespace Selfaware.Features.Quizzes.DTOs
{
    public class BulkQuizUploadDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int TimeLimitInMinutes { get; set; }

        public IFormFile File { get; set; } = null!;
    }
}
