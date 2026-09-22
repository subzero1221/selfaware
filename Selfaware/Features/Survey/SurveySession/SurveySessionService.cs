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

        public async Task<ServiceResult<NextQuestionResponseDto>> GetQuestionAsync(Guid surveyId, int order)
        {
            var surveyExists = await _context.Surveys.AnyAsync(s => s.Id == surveyId);
            if (!surveyExists)
                return ServiceResult<NextQuestionResponseDto>.Failed("Survey not found");

            var nextQuestionDto = await _context.Surveys
                .Where(s => s.Id == surveyId)
                .SelectMany(s => s.Quiz.Questions)
                .Where(q => q.Order > order)
                .OrderBy(q => q.Order)
                .Select(q => new QuestionResultDto(
                    q.Id, q.Text, q.Order,
                    _context.Answers.Count(a => a.QuestionId == q.Id && a.SurveySession.SurveyId == surveyId),
                    q.Options.OrderBy(o => o.Id).Select(opt => new OptionResultDto(
                        opt.Id, opt.Text,
                        _context.Answers.Count(a => a.OptionId == opt.Id && a.SurveySession.SurveyId == surveyId)
                    )).ToList(),
                    q.ImageUrl, q.ImagePublicId
                ))
                .FirstOrDefaultAsync();

            if (nextQuestionDto == null)
            {
              
                var hasQuestions = await _context.Surveys
                    .Where(s => s.Id == surveyId)
                    .SelectMany(s => s.Quiz.Questions)
                    .AnyAsync();

                if (hasQuestions)
                {
                 
                    return ServiceResult<NextQuestionResponseDto>.Ok(
                        new NextQuestionResponseDto(null, true),
                        "Survey completed"
                    );
                }

                return ServiceResult<NextQuestionResponseDto>.Failed("Survey has no questions");
            }


            return ServiceResult<NextQuestionResponseDto>.Ok(
                new NextQuestionResponseDto(nextQuestionDto, false),
                "Question fetched successfully"
            );
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
                SurveySessionId = dto.SurveySessionId,
                QuestionId = dto.QuestionId,
                OptionId = dto.OptionId,
                SubmittedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            var submittedAnswer = new UserAnswerDto(
                Id: answerId,
                SurveySessionId:answer.SurveySessionId,
                QuestionId: answer.QuestionId,
                OptionId: answer.OptionId,
                SubmittedAt: answer.SubmittedAt,
                UserId: userId?.ToString()
            );

            return ServiceResult<UserAnswerDto>.Ok(submittedAnswer, "Your answer was submitted successfully.");
        }

    }
}
