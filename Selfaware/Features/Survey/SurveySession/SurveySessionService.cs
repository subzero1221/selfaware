using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Survey.SurveySession.Dtos;
using Selfaware.Features.Survey.DTOs;
using Selfaware.Features.Survey.SurveySession.Entities;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.Models;


namespace Selfaware.Features.Survey.SurveySession
{
    public class SurveySessionService : ISurveySessionService
    {
        public readonly AppDbContext _context;

        public SurveySessionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<SurveySessionDto>> StartSurveySessionAsync(StartSurveySessionDto dto)
        {

            var survey = await _context.Surveys
                            .Where(s => s.Id == dto.SurveyId)
                            .Select(s => new { s.Id, s.AllowAnonymous, s.IsActive })
                            .FirstOrDefaultAsync();

            if (survey == null)
            {
                return ServiceResult<SurveySessionDto>.Failed("Survey not found");
            }

            if (!survey.IsActive)
            {
                return ServiceResult<SurveySessionDto>.Failed("Survey is currently inactive");
            }

            
            if (!survey.AllowAnonymous && string.IsNullOrWhiteSpace(dto.Email))
            {
                return ServiceResult<SurveySessionDto>.Failed("For Session start please provide Email");
            }

            Guid surveySessionId = Guid.NewGuid();
            Guid anonymousToken = Guid.NewGuid();


            var surveySession = new Entities.SurveySession
            {
                Id = surveySessionId,
                SurveyId = dto.SurveyId,
                AnonymousToken = anonymousToken.ToString(),
                Nickname = dto.NickName ?? null,
                Email = dto.Email??null
            };

            _context.SurveySessions.Add(surveySession);
            await _context.SaveChangesAsync();

            var newSession = await GetSurveySessionAsync(surveySession.Id);

            if (newSession.Data == null)
            {
                return ServiceResult<SurveySessionDto>.Failed("Session not found");
            }


            return ServiceResult<SurveySessionDto>.Ok(newSession.Data, "Survey Session started succesfully");

        }

        public async Task<ServiceResult<SurveySessionDto>> GetSurveySessionAsync(Guid id)
        {
            var surveySession = await _context.SurveySessions
                    .Where(s => s.Id == id)
                    .Select(s => new SurveySessionDto(
                        s.Id,
                        s.SurveyId,
                        s.AnonymousToken,
                        s.StartedAt,
                        s.IsCompleted,
                        s.Nickname ?? "",
                        s.Email??null,
                        new SurveyDto(
                            s.Survey.Id,
                            s.Survey.RunById,
                            new QuizForSurveyDto(
                                s.Survey.Quiz.Id,
                                s.Survey.Quiz.QuizType,
                                s.Survey.Quiz.QuizStatus,
                                s.Survey.Quiz.Questions.Count,
                                s.Survey.Quiz.Questions.OrderBy(q => q.Order).Select(q => new QuestionDto(
                                    q.Id,
                                    q.Text,
                                    q.Type,
                                    q.Order,
                                    q.Options.Select(o => new OptionDto(o.Id, o.Text)).ToList()
                                )).ToList()
                            ),
                            s.Survey.ShareCode,
                            s.Survey.CompletedBy,
                            s.Survey.IsActive,
                            s.Survey.AllowAnonymous,
                            s.Survey.ExpiresAt,
                            s.Survey.CreatedAt,
                            s.Survey.LastActivatedAt
                        ),
                        s.UserId,
                        s.CompletedAt
                    ))
                .FirstOrDefaultAsync();

            if(surveySession == null)
            {
                return ServiceResult<SurveySessionDto>.Failed("Session not found");
            }

            return ServiceResult<SurveySessionDto>.Ok(surveySession, "Survey Session fetched successfully");

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


        public async Task<ServiceResult<SurveySessionResultDto>> GetSurveySessionResultAsync(Guid surveySessionId)
        {
            var surveySession = await _context.SurveySessions
          .FirstOrDefaultAsync(s => s.Id == surveySessionId);

            if (surveySession == null)
                return ServiceResult<SurveySessionResultDto>.Failed("Survey session not found");

            var targetSurveyId = surveySession.SurveyId;

            var SurveyQuestionsDto = await _context.SurveySessions
                .Where(s => s.Id == surveySessionId)
                .Select(s => s.Survey.Quiz.Questions
                .Select(q => new QuestionResultDto(
                    q.Id, q.Text, q.Order,
                    _context.Answers.Count(a => a.QuestionId == q.Id && a.SurveySession.SurveyId == targetSurveyId),
                    q.Options.OrderBy(o => o.Id).Select(opt => new OptionResultDto(
                        opt.Id, opt.Text,
                        _context.Answers.Count(a => a.OptionId == opt.Id && a.SurveySession.SurveyId == targetSurveyId)
                    )).ToList(),
                    q.ImageUrl, q.ImagePublicId
                )))
                .FirstOrDefaultAsync();

            if (SurveyQuestionsDto == null)
            {
                return ServiceResult<SurveySessionResultDto>.Failed("Questions not found");
            }

            var userAnswers = await _context.Answers
                     .Where(a => a.SurveySessionId == surveySessionId)
                     .Select(a => new UserAnswerDto(
                                  a.Id,
                                  a.SurveySessionId,
                                  a.QuestionId,
                                  a.OptionId,
                                  a.SubmittedAt
                              ))
                                .ToListAsync();



            if (userAnswers == null)
            {
                return ServiceResult<SurveySessionResultDto>.Failed("Your answers not found");
            }

            return ServiceResult<SurveySessionResultDto>.Ok(
               new SurveySessionResultDto(SurveyQuestionsDto, userAnswers),
                "Session Result fetched successfully"
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
                SurveySessionId: answer.SurveySessionId,
                QuestionId: answer.QuestionId,
                OptionId: answer.OptionId,
                SubmittedAt: answer.SubmittedAt,
                UserId: userId?.ToString()
            );

            return ServiceResult<UserAnswerDto>.Ok(submittedAnswer, "Your answer was submitted successfully.");
        }

    }
}
