using Selfaware.Shared.Models;
using Selfaware.Features.Survey.DTOs;

namespace Selfaware.Features.Survey
{
    public interface ISurveyService
    {
        Task<ServiceResult<SurveyDto>> ActivateSurveyAsync(ActivateSurveyDto dto, string userId);
        Task<ServiceResult<List<SurveyDto>>> GetMyActiveSurveysAsync(string userId);
    }
}
