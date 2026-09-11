using Selfaware.Features.Quizzes;
using Selfaware.Features.Survey.DTOs;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.Models;
using NanoidDotNet;



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


        public async Task<ServiceResult<SurveyDto>> ActivateSurveyAsync(ActivateSurveyDto dto)
        {
            Guid surveyId = Guid.NewGuid();
            var quiz = await _quizService.GetQuizForSurvey(dto.QuizId);
            if (!quiz.Success)
            {
                return ServiceResult<SurveyDto>.Failed(quiz.Message);
            }

            string shareCode = Nanoid.Generate(size: 8);

            DateTime? expiresAt = dto.DurationInDays.HasValue
            ? DateTime.UtcNow.AddDays(dto.DurationInDays.Value)
            : null;

            var survey = new Entities.Survey
            {
                Id = surveyId,
                QuizId = dto.QuizId,
                ShareCode = shareCode,
                IsActive = true,
                CompletedBy = 0,
                AllowAnonymous = dto.AllowAnonymous,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                LastActivatedAt = DateTime.UtcNow
             
            };

            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();

            var activatedSurvey = new SurveyDto
                (
                SurveyId: surveyId,
                Quiz: quiz.Data,
                ShareCode:shareCode,
                CompletedBy:0,
                IsActive:true,
                AllowAnonymous:dto.AllowAnonymous,
                ExpiresAt:expiresAt,
                CreatedAt:DateTime.UtcNow,
                LastActivatedAt:DateTime.UtcNow
                
                );

            return ServiceResult<SurveyDto>.Ok(activatedSurvey, "Survey Activated successfully");

        }

    }
}
