using Selfaware.Shared.Models;
using Selfaware.Features.Quizzes.DTOs;


namespace Selfaware.Features.Quizzes
{
    public interface IQuizEditorService
    {
        Task<ServiceResult<Guid>> EditSettingsAsync(Guid quizId, string userId, EditQuizSettingsDto dto);
        Task<ServiceResult<Guid>> EditQuestionAsync(Guid quizId, Guid questionId, string userId, EditQuestionDto dto);

    }
}
