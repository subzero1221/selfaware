using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.DTOs.Selfaware.Features.Quizzes.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Quizzes
{
    public interface IQuizService
    {
        Task<ServiceResult<QuizDto>> CreateQuizAsync(CreateQuizDto model);
       // Task<ServiceResult<QuizSummaryDto>> BulkImportQuizAsync(string title, int timeLimitInMinutes, Stream fileStream, string Description, string userId);
        Task<ServiceResult<GetQuizzesDto>> GetMyQuizzesAsync(string userId);

        Task<ServiceResult<Guid>> PutQuizAsync(PutQuizDto dto,Guid quizId, string userId);
        Task<ServiceResult<QuizDetailsDto>> GetSingleQuizAsync(Guid quizId, string userId);

        Task<ServiceResult<Guid>> DeleteQuizAsync(Guid quizId, string userId);
    }
}
