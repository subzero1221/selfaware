using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using Selfaware.Features.Quizzes;
using Selfaware.Features.Quizzes.DTOs;
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


        public async Task<ServiceResult<SurveyDto>> ActivateSurveyAsync(ActivateSurveyDto dto, string userId)
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
                RunById = userId,
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
                RunById:userId,
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

        public async Task<ServiceResult<Guid>> DeactivateSurveyAsync(Guid surveyId, string userId)
        {
            var rowsAffected = await _context.Surveys
         .Where(s => s.Id == surveyId && s.RunById == userId && s.IsActive)
         .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsActive, false));

            if (rowsAffected == 0)
            {
                return ServiceResult<Guid>.Failed("Survey not found or already inactive");
            }

            return ServiceResult<Guid>.Ok(surveyId, "Survey deactivated successfully");

        }

        public async Task<ServiceResult<List<SurveyDto>>> GetMyActiveSurveysAsync(string userId)
        {
            var query = _context.Surveys.AsNoTracking().Where(survey => survey.RunById == userId && survey.IsActive == true).Include(survey=>survey.Quiz);

            var surveyList = await query.Select(survey => new SurveyDto(
                SurveyId: survey.Id,
                RunById: survey.RunById,
                Quiz: new QuizForSurveyDto(
                     QuizId: survey.Quiz.Id,
                     QuizType: survey.Quiz.QuizType,
                     QuizStatus: survey.Quiz.QuizStatus,
                     QuestionCount: survey.Quiz.Questions.Count,
                     Questions:null,
                     Description: survey.Quiz.Description,
                     Title: survey.Quiz.Title
                            ),
               ShareCode: survey.ShareCode,
               CompletedBy: survey.CompletedBy,
               IsActive: survey.IsActive,
               AllowAnonymous: survey.AllowAnonymous,
               ExpiresAt: survey.ExpiresAt,
               CreatedAt: survey.CreatedAt,
               LastActivatedAt: survey.LastActivatedAt
                )).ToListAsync();



            return ServiceResult<List<SurveyDto>>.Ok(surveyList, "Surveys fetched succesfully");
        }

    }
}
