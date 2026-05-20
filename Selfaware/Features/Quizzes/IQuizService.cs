using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Quizzes
{
    public interface IQuizService
    {
        Task<ServiceResult<QuizDto>> CreateQuizAsync(CreateQuizDto model);
        Task<ServiceResult<QuizUploadedResponseDto>> BulkImportQuizAsync(string title, int timeLimitInMinutes, Stream fileStream, string Description);
    }
}
