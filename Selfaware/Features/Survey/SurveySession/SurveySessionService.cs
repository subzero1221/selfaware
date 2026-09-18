using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Survey.SurveySession.Dtos;
using Selfaware.Features.Survey.SurveySession.Entities;
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
                SurveyId = dto.SurveyId,
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

        public async Task<ServiceResult<SurveySessionDto>> GetSurveySessionAsync(Guid id)
        {
            var surveySession = await _context.SurveySessions.AsNoTracking().Where(surveySession => surveySession.Id == id).Include(surveySession=> surveySession.Survey).ThenInclude(s=>s.Quiz).FirstOrDefaultAsync();
            if(surveySession == null)
            {
                return ServiceResult<SurveySessionDto>.Failed("Survey not found");
            }
            var surveySessionDto = new SurveySessionDto(
                Id: surveySession.Id,
                SurveyId: surveySession.SurveyId,
                AnonymousToken: surveySession.AnonymousToken,
                StartedAt: surveySession.StartedAt,
                IsCompleted: surveySession.IsCompleted,
                Survey: surveySession.Survey,
                Nickname: surveySession.Nickname,
                UserId: surveySession.UserId,
                CompletedAt: surveySession.CompletedAt
                );

            return ServiceResult<SurveySessionDto>.Ok(surveySessionDto, "Survey Session fetched successfully");

        }

        public async Task<ServiceResult<QuestionResultDto>> GetFirstQuestionAsync(Guid surveyId)
        {
        
            var surveyExists = await _context.Surveys.AnyAsync(s => s.Id == surveyId);
            if (!surveyExists)
            {
                return ServiceResult<QuestionResultDto>.Failed("Survey not found");
            }

        
            var firstQuestionDto = await _context.Surveys
                .Where(s => s.Id == surveyId)
                .SelectMany(s => s.Quiz.Questions)
                .OrderBy(q => q.Order) 
                .Select(q => new QuestionResultDto(
                    q.Id,
                    q.Text,
                    q.Order,
                _context.Answers.Count(a =>
                a.OptionId == q.Id &&
                a.SurveySession.SurveyId == surveyId
            ),
                    q.Options
                    .OrderBy(o => o.Id)
                    .Select(opt => new OptionResultDto(
                        opt.Id,
                        opt.Text,
                        _context.Answers.Count(a =>
                a.OptionId == opt.Id &&
                a.SurveySession.SurveyId == surveyId
            )
                    )).ToList(),
                    q.ImageUrl,
                    q.ImagePublicId
                ))
                .FirstOrDefaultAsync();

            if (firstQuestionDto == null)
            {
                return ServiceResult<QuestionResultDto>.Failed("Question not found");
            }

            return ServiceResult<QuestionResultDto>.Ok(firstQuestionDto, "Question fetched successfully");
        }


        public async Task<ServiceResult<UserAnswerDto>> SubmitAnswerAsync(SubmitSurveyAnswerDto dto)
        {

            Guid? userId = null;
            if (!string.IsNullOrEmpty(dto.UserId) && Guid.TryParse(dto.UserId, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var answerId = Guid.NewGuid();
            var answer = new UserAnswer
            {
                Id = answerId,
                QuestionId = dto.QuestionId,
                OptionId = dto.OptionId,
                SubmittedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            var submittedAnswer = new UserAnswerDto(
                Id: answerId,
                QuestionId: answer.QuestionId,
                OptionId: answer.OptionId,
                SubmittedAt: answer.SubmittedAt,
                UserId: userId?.ToString()
            );

            return ServiceResult<UserAnswerDto>.Ok(submittedAnswer, "Your answer was submitted successfully.");
        }

    }
}
