using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.DTOs.Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Features.Quizzes.Enums;
using Selfaware.Features.Quizzes.Parsers;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.Models;
using System.Xml;


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
            if (exists) return ServiceResult<QuizDto>.Failed("A quiz with this title already exists.");

            var quiz = new Quiz
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                QuestionCount = dto.Questions.Count,
                TimeLimit = TimeSpan.FromMinutes(dto.TimeLimitInMinutes),
                Questions = dto.Questions.Select((question, index) => new Question
                {
                    Id = Guid.NewGuid(),
                    Text = question.Text,
                    Type = question.QuestionType,
                    Order = index + 1,
                    Options = question.Options.Select((o, index) => new Option
                    {
                        Text = o.Text,
                        Score = o.Score
                    }).ToList()

                }).ToList()
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return ServiceResult<QuizDto>.Ok(new QuizDto
            (
                QuizId : quiz.Id,
                Title : quiz.Title,
                Description : quiz.Description,
                TimeLimitInMinutes : dto.TimeLimitInMinutes,
                QuestionCount : quiz.QuestionCount
            ), "Quiz created successfully");
        }

       public async Task<ServiceResult<Guid>> PutQuizAsync(PutQuizDto dto, Guid quizId, string userId) 
        {
            var existingQuiz = await _context.Quizzes
              .Include(q => q.Questions)
              .ThenInclude(q => q.Options)
              .FirstOrDefaultAsync(q => q.Id == quizId && q.CreatedById == userId);

            if (existingQuiz == null)
            {
                return ServiceResult<Guid>.Failed("Quiz not Found");
            }

            existingQuiz.Title = dto.Title;
            existingQuiz.Description = dto.Description;
            existingQuiz.Slug = dto.Slug;
            existingQuiz.TimeLimit = TimeSpan.FromMinutes(dto.TimeLimit);
            existingQuiz.QuizStatus = dto.QuizStatus;
            existingQuiz.QuestionCount = dto.QuestionCount;
           
            existingQuiz.Questions.Clear();
           

            existingQuiz.Questions = dto.Questions.Select((qDto, Index) => new Question
            {
                Id = Guid.NewGuid(),
           
                Text = qDto.Text,
                Order = Index + 1,
                Type = qDto.Type,
                Options = qDto.Options.Select(oDto => new Option
                {
                    Id = Guid.NewGuid(),
                    Text = oDto.Text,
               
                    Score = oDto.Score
                }).ToList()
            }).ToList();

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
            var quizzes = await _context.Quizzes.AsNoTracking()
           .Where(q => q.CreatedById == userId)
            .Select(q => new QuizSummaryDto
            (
                Id: q.Id,
                QuestionCount: q.Questions.Count,
                QuizStatus: q.QuizStatus,
                Title: q.Title,
                Slug: q.Slug,
                Description: q.Description,
                QuizType: q.QuizType
                
            )).ToListAsync();

            var quizzesResult = new GetQuizzesDto
            (
                Quizzes : quizzes,
                TotalCount : quizzes.Count
            );

            return ServiceResult<GetQuizzesDto>.Ok(quizzesResult, "Quizzes success");
        }
        public async Task<ServiceResult<QuizDetailsDto>> GetSingleQuizAsync(Guid quizId, string userId)
        {
            
            var quizEntity = await _context.Quizzes
                .AsNoTracking()
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
                QuizStatus:quizEntity.QuizStatus,
                Title: quizEntity.Title,
                Description: quizEntity.Description,
                Slug:quizEntity.Slug,
                Questions: quizEntity.Questions.Select(question => new QuestionDto(
                    Id: question.Id,
                    Text: question.Text,
                    Type: question.Type,
                    Order:question.Order,
                    Options: question.Options.Select(option => new OptionDto(
                        Text: option.Text,
                        Score: option.Score
                    )).ToList()
                )).ToList()
            );

            return ServiceResult<QuizDetailsDto>.Ok(quizDto, "Quiz fetched succesfully");

        }
    }
}
