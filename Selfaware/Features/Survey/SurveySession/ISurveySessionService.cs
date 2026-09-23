using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Survey.SurveySession.Dtos;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Survey.SurveySession
{
    public interface ISurveySessionService
    {
        public Task<ServiceResult<SurveySessionDto>> StartSurveySessionAsync(StartSurveySessionDto dto);

        public Task<ServiceResult<SurveySessionDto>> GetSurveySessionAsync(Guid id);
        public Task<ServiceResult<NextQuestionResponseDto>> GetQuestionAsync(Guid surveyId, int order);
        public Task<ServiceResult<SurveySessionResultDto>> GetSurveySessionResultAsync(Guid surveySessionId);

        public Task<ServiceResult<UserAnswerDto>> SubmitAnswerAsync(SubmitSurveyAnswerDto dto);
    }
}
