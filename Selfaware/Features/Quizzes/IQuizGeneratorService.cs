using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.DTOs.Selfaware.Features.Quizzes.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Quizzes
{
    public interface IQuizGeneratorService
    {
        Task<ServiceResult<Guid>> ExtractExistingQuizAsync(ExtractQuizRequestDto dto, string currentUserId, CancellationToken cancellationToken);
        
    }
}
