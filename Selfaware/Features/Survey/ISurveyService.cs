using Selfaware.Shared.Models;
using Selfaware.Features.Survey.DTOs;

namespace Selfaware.Features.Survey
{
    public interface ISurveyService
    {
        Task<ServiceResult<SurveyDto>> StartSurvey(Guid quizId);
    }
}
