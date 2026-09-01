using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Quizzes;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Enums;
using Selfaware.Features.Survey.DTOs;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.Models;



namespace Selfaware.Features.Survey
{
    public class SurveyService:ISurveyService
    {
        private readonly AppDbContext _context;
        private readonly IQuizService _quizService;

        public SurveyService(AppDbContext context, IQuizService quizService)
        {
            _context = context;
            _quizService = quizService;
        }


        public async Task<ServiceResult<SurveyDto>> StartSurvey(Guid quizId)
        {
            Guid surveyId = Guid.NewGuid();
            var quiz = await _quizService.GetQuizForSurvey(quizId);
            if (!quiz.Success)
            {
                return ServiceResult<SurveyDto>.Failed(quiz.Message);
            }
            var survey = new SurveyDto
                (
                SurveyId: surveyId,
                Quiz: quiz.Data,
                Status: 0
                );

            return ServiceResult<SurveyDto>.Ok(survey, "Survey created successfully");

        }

    }
}
