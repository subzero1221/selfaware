using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Survey.SurveySession.Dtos;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Survey.SurveySession
{
    public interface ISurveySessionService
    {
        public Task<ServiceResult<SurveySessionDto>> StartSurveySessionAsync(StartSurveySessionDto dto);
        public Task<ServiceResult<QuestionDto>> GetQuestionForSurveySessionAsync(GetQuestionForSurveySessionDto dto);
    }
}
