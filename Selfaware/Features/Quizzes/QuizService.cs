using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Infrastructure.Data;
using Selfaware.Features.Quizzes.Entities;
using System.Text.Json;
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
            if (exists) return ServiceResult<QuizDto>.Failed("A quiz with this title already exists.");

            var quiz = new Quiz
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                QuestionCount = dto.Questions.Count,
                TimeLimit = TimeSpan.FromMinutes(dto.TimeLimitInMinutes),
                Questions = dto.Questions.Select((q, index) => new Question
                {
                    Id = Guid.NewGuid(),
                    Text = q.Text,
                    QuestionType = q.QuestionType,
                    OptionsJson = JsonSerializer.Serialize(q.OptionsJson),
                    Order = index + 1
                }).ToList()
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return ServiceResult<QuizDto>.Ok(new QuizDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                TimeLimitInMinutes = dto.TimeLimitInMinutes,
                QuestionCount = dto.QuestionsCount,
            }, "Quiz created successfully");
        }

        public async Task<ServiceResult<QuizSummaryDto>> BulkImportQuizAsync(string title, int timeLimitInMinutes, Stream fileStream, string Description, string userId)
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
                QuestionType = "SingleChoice",
                OptionsJson = JsonSerializer.Serialize(q.Options),
                CorrectAnswerIndex = q.CorrectAnswerIndex
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
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                Slug = quiz.Slug,
                QuestionCount = quiz.QuestionCount,
                QuizType = quiz.QuizType
            };

            return ServiceResult<QuizSummaryDto>.Ok(data, "Quiz created succesfuly");
        }
        public async Task<ServiceResult<GetMyQuizzesDto>> GetMyQuizzesAsync(string userId)
        {
            var quizzes = await _context.Quizzes
           .Where(q => q.CreatedById == userId)
            .Select(q => new QuizSummaryDto
            {
                Id = q.Id,
                Title = q.Title,
                Slug = q.Slug,
                Description = q.Description,
                QuestionCount = q.QuestionCount,
                QuizType = q.QuizType
            }).ToListAsync();

            var quizzesResult = new GetMyQuizzesDto
            {
                Quizzes = quizzes,
                TotalCount = quizzes.Count
            };

            return ServiceResult<GetMyQuizzesDto>.Ok(quizzesResult, "Quizzes success");
        }
        public async Task<ServiceResult<QuizDetailsDto?>> GetSingleQuizAsync(Guid quizId, string userId)
        {
            var quiz = await _context.Quizzes.Include(q => q.Questions).Where(q => q.Id == quizId && q.CreatedById == userId).Select(q => new QuizDetailsDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                TimeLimit = q.TimeLimit,
                Questions = q.Questions.Select(question => new QuestionDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    QuestionType = question.QuestionType,
                    OptionsJson = question.OptionsJson
                }).ToList()
            })
        .FirstOrDefaultAsync();

            return ServiceResult<QuizDetailsDto>.Ok(quiz, "Quiz fetched succesfully");

        }
    }
}
