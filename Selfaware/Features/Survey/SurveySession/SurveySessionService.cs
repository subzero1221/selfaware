using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Features.Quizzes.Enums;
using Selfaware.Features.Survey.SurveySession.Dtos;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.Models;


namespace Selfaware.Features.Survey.SurveySession
{
    public class SurveySessionService:ISurveySessionService
    {
        public readonly AppDbContext _context;

        public SurveySessionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<SurveySessionDto>> StartSurveySessionAsync(StartSurveySessionDto dto)
        {

            Guid surveySessionId = Guid.NewGuid();
            Guid anonymousToken = Guid.NewGuid();

            var surveySession = new Entities.SurveySession
            {
                Id = surveySessionId,
                SurveyId = surveySessionId,
                AnonymousToken = anonymousToken.ToString(),
                Nickname = dto.NickName??null,
            };

            _context.SurveySessions.Add(surveySession);
            await _context.SaveChangesAsync();

            var newSession = new SurveySessionDto
                (
                Id: surveySession.Id,
                SurveyId:surveySession.SurveyId,
                AnonymousToken:surveySession.AnonymousToken,
                StartedAt:surveySession.StartedAt,
                IsCompleted:surveySession.IsCompleted,
                Nickname:surveySession.Nickname,
                UserId:surveySession.UserId,
                CompletedAt:surveySession.CompletedAt
                );

            return ServiceResult<SurveySessionDto>.Ok(newSession, "Survey Session started succesfully");

        }

        public async Task<ServiceResult<QuestionDto>> GetFirstQuestionAsync(string shareCode)
        {
        
            var surveyExists = await _context.Surveys.AnyAsync(s => s.ShareCode == shareCode);
            if (!surveyExists)
            {
                return ServiceResult<QuestionDto>.Failed("Survey not found");
            }

        
            var firstQuestionDto = await _context.Surveys
                .Where(s => s.ShareCode == shareCode)
                .SelectMany(s => s.Quiz.Questions)
                .OrderBy(q => q.Order) 
                .Select(q => new QuestionDto(
                    q.Id,
                    q.Text,
                    q.Type,
                    q.Order,
                    q.Options.Select(opt => new OptionDto(
                        opt.Id,
                        opt.Text,
                        null, 
                        opt.VoteCount
                    )).ToList(),
                    q.ImageUrl,
                    q.ImagePublicId
                ))
                .FirstOrDefaultAsync();

            if (firstQuestionDto == null)
            {
                return ServiceResult<QuestionDto>.Failed("Question not found");
            }

            return ServiceResult<QuestionDto>.Ok(firstQuestionDto, "Question fetched successfully");
        }

    }
}
