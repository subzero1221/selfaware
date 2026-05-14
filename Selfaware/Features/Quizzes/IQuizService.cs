using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes;

namespace Selfaware.Features.Quizzes
{
    public interface IQuizService
    {
        Task<Guid> CreateQuizAsync(CreateQuizDto model);
    }
}
