using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.DTOs.Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Features.Quizzes.Parsers;
using Selfaware.Features.Survey.DTOs;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Quizzes
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;
        private readonly IQuizCsvParser _csvParser;

        public QuizService(AppDbContext context, IQuizCsvParser csvParser)
        {
            _context = context;
            _csvParser = csvParser;
        }

        public async Task<ServiceResult<QuizDto>> CreateQuizAsync(CreateQuizDto dto)
        {
            bool exists = await _context.Quizzes.AnyAsync(q => q.Title == dto.Title);
            if (exists)
                return ServiceResult<QuizDto>.Failed("A quiz with this title already exists.");

            var quiz = new Quiz
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                QuestionCount = dto.Questions.Count,
                TimeLimit = dto.TimeLimitInMinutes,
                Questions = dto
                    .Questions.Select(
                        (question, index) =>
                            new Question
                            {
                                Id = Guid.NewGuid(),
                                Text = question.Text,
                                Type = question.QuestionType,
                                Order = index + 1,
                                Options = question
                                    .Options.Select(
                                        (o, index) => new Option { Text = o.Text, Score = o.Score }
                                    )
                                    .ToList(),

                                ImageUrl = question.ImageUrl,
                                ImagePublicId = question.ImagePublicId,
                            }


                    )
                    .ToList(),
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return ServiceResult<QuizDto>.Ok(
                new QuizDto(
                    QuizId: quiz.Id,
                    Title: quiz.Title,
                    Description: quiz.Description,
                    TimeLimitInMinutes: dto.TimeLimitInMinutes,
                    QuizType:dto.QuizType,
                    QuestionCount: quiz.QuestionCount
                ),
                "Quiz created successfully"
            );
        }

        public async Task<ServiceResult<Guid>> PutQuizAsync(
            PutQuizDto dto,
            Guid quizId,
            string userId
        )
        {
            var existingQuiz = await _context
                .Quizzes.Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.CreatedById == userId);

            if (existingQuiz == null)
            {
                return ServiceResult<Guid>.Failed("Quiz not Found or Access Denied");
            }

            existingQuiz.Title = dto.Title;
            existingQuiz.Description = dto.Description;
            existingQuiz.Slug = dto.Slug;
            existingQuiz.TimeLimit = dto.TimeLimit;
            existingQuiz.QuizStatus = dto.QuizStatus;
            existingQuiz.QuestionCount = dto.QuestionCount;

            _context.Questions.RemoveRange(existingQuiz.Questions);

            var newQuestions = dto
                .Questions.Select(
                    (qDto, Index) =>
                        new Question
                        {
                            Id = Guid.NewGuid(),
                            QuizId = existingQuiz.Id,
                            Text = qDto.Text,
                            Order = Index + 1,
                            Type = qDto.Type,
                            Options = qDto
                                .Options.Select(oDto => new Option
                                {
                                    Id = Guid.NewGuid(),
                                    Text = oDto.Text,

                                    Score = oDto.Score,
                                })
                                .ToList(),
                            ImageUrl = qDto.ImageUrl,
                            ImagePublicId = qDto.ImagePublicId,
                        }
                )
                .ToList();

            _context.Questions.AddRange(newQuestions);

            await _context.SaveChangesAsync();

            return ServiceResult<Guid>.Ok(existingQuiz.Id, "Quiz saved succesfully");
        }

        /* public async Task<ServiceResult<QuizSummaryDto>> BulkImportQuizAsync(string title, int timeLimitInMinutes, Stream fileStream, string Description, string userId)
         {
             var parsedQuestions = _csvParser.ParseQuestionsFromStream(fileStream);
             if (!parsedQuestions.Any()) return ServiceResult<QuizSummaryDto>.Failed("No valid questions found in the CSV.");


             var quizId = Guid.NewGuid();

             var questionsEntities = parsedQuestions.Select((q, index) => new Question
             {
                 Id = Guid.NewGuid(),
                 QuizId = quizId,
                 Text = q.Text,
                 Order = index + 1,
                 Type = Enums.QuestionType.SingleChoice,
                 Options = q.Options.Select((option, index) =>
                 {
                   Text = option.Text,
                   Score = option.Score
                 }).ToList()
             }).ToList();

             var quiz = new Quiz
             {
                 Id = quizId,
                 Title = title,
                 Description = Description,
                 Slug = title.ToLower().Replace(" ", "-").Trim(),
                 CreatedAt = DateTime.UtcNow,
                 CreatedById = userId,
                 QuestionCount = questionsEntities.Count,

                 TimeLimit = TimeSpan.FromMinutes(timeLimitInMinutes),
                 Questions = questionsEntities,
             };

             _context.Quizzes.Add(quiz);
             await _context.SaveChangesAsync();

             var data = new QuizSummaryDto
             (
                 Id : quiz.Id,
                 Title : quiz.Title,
                 Description: quiz.Description,
                 Slug: quiz.Slug,
                 QuestionCount: quiz.QuestionCount,
                 QuizType: quiz.QuizType
             );

             return ServiceResult<QuizSummaryDto>.Ok(data, "Quiz created succesfuly");
         }*/

        public async Task<ServiceResult<GetQuizzesDto>> GetMyQuizzesAsync(string userId)
        {
            var quizzes = await _context
                .Quizzes.AsNoTracking()
                .Where(q => q.CreatedById == userId)
                .Select(q => new QuizSummaryDto(
                    Id: q.Id,
                    QuestionCount: q.Questions.Count,
                    QuizStatus: q.QuizStatus,
                    QuizType:q.QuizType,
                    Title: q.Title,
                    Slug: q.Slug,
                    Description: q.Description
                ))
                .ToListAsync();

            var quizzesResult = new GetQuizzesDto(Quizzes: quizzes, TotalCount: quizzes.Count);

            return ServiceResult<GetQuizzesDto>.Ok(quizzesResult, "Quizzes success");
        }

        public async Task<ServiceResult<QuizDetailsDto>> GetSingleQuizAsync(
            Guid quizId,
            string userId
        )
        {
            var quizEntity = await _context
                .Quizzes.AsNoTracking()
                .Include(q => q.Questions)
                    .ThenInclude(question => question.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.CreatedById == userId);

            if (quizEntity == null)
            {
                return ServiceResult<QuizDetailsDto>.Failed("Quiz not found");
            }

            var quizDto = new QuizDetailsDto(
                Id: quizEntity.Id,
                TimeLimit: quizEntity.TimeLimit,
                QuizStatus: quizEntity.QuizStatus,
                QuizType:quizEntity.QuizType,
                Title: quizEntity.Title,
                Description: quizEntity.Description,
                Slug: quizEntity.Slug,
                Questions: quizEntity
                    .Questions.Select(question => new QuestionDto(
                        Id: question.Id,
                        Text: question.Text,
                        Type: question.Type,
                        Order: question.Order,
                        Options: question
                            .Options.Select(option => new OptionDto(
                                Id: option.Id,
                                Text: option.Text,
                                Score: option.Score
                            ))
                            .ToList(),
                        ImageUrl:question.ImageUrl,
                        ImagePublicId:question.ImagePublicId
                    ))
                    .ToList()
            );

            return ServiceResult<QuizDetailsDto>.Ok(quizDto, "Quiz fetched succesfully");
        }

        public async Task<ServiceResult<Guid>> DeleteQuizAsync(Guid quizId, string userId)
        {
            var affectedRows = await _context
                .Quizzes.Where(quiz => quiz.Id == quizId && quiz.CreatedById == userId)
                .ExecuteDeleteAsync();

            if (affectedRows == 0)
            {
                return ServiceResult<Guid>.Failed(
                    "Quiz not found or you don't have permission to delete it."
                );
            }

            return ServiceResult<Guid>.Ok(quizId, "Quiz Deleted succesfully");
        }

        public async Task<ServiceResult<QuizForSurveyDto>> GetQuizForSurvey(Guid quizId)
        {

         var quizEntity = await _context
        .Quizzes.AsNoTracking()
        .Include(q => q.Questions)
            .ThenInclude(question => question.Options)
        .FirstOrDefaultAsync(q => q.Id == quizId && q.QuizType == QuizType.Survey);

            if (quizEntity == null)
            {
                return ServiceResult<QuizForSurveyDto>.Failed("Survey not found or is not active");
            }



            var quizForSurvey = new QuizForSurveyDto(
                QuizId: quizEntity.Id,
                QuizStatus: quizEntity.QuizStatus,
                QuizType: quizEntity.QuizType,
                Title: quizEntity.Title,
                Description: quizEntity.Description,
                QuestionCount: quizEntity.Questions.Count,
                Questions: quizEntity
                    .Questions.Select(question => new QuestionDto(
                        Id: question.Id,
                        Text: question.Text,
                        Type: question.Type,
                        Order: question.Order,
                        Options: question
                            .Options.Select(option => new OptionDto(
                                Id: option.Id,
                                Text: option.Text,
                                Score: option.Score
                            ))
                            .ToList(),
                        ImageUrl: question.ImageUrl,
                        ImagePublicId: question.ImagePublicId
                    ))
                    .ToList()
            );

            return ServiceResult<QuizForSurveyDto>.Ok(quizForSurvey, "Quiz for survey fetched succesfully");
        }
    }
}
