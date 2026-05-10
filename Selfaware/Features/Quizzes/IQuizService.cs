using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Models.Entities;

namespace Selfaware.Features.Quizzes
{
    public interface IQuizService
    {
        Task<Guid> CreateQuizAsync(CreateQuizDto model);
    }
}
