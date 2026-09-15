using Selfaware.Shared.Models;
using Selfaware.Features.Survey.DTOs;
using Selfaware.Features.Quizzes.DTOs;

namespace Selfaware.Features.Survey
{
    public interface ISurveyService
    {
        Task<ServiceResult<SurveyDto>> ActivateSurveyAsync(ActivateSurveyDto dto, string userId);
        Task<ServiceResult<Guid>> DeactivateSurveyAsync(Guid surveyId, string userId);
        Task<ServiceResult<List<SurveyDto>>> GetMyActiveSurveysAsync(string userId);

        Task<ServiceResult<SurveyDto>> GetSurveyAsync(string shareCode);

    }
}
