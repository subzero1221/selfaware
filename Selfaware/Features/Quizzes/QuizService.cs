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

            return ServiceResult<QuizDto>.Ok(new QuizDto { 
             Id = quiz.Id,
             Title = quiz.Title,
             Description = quiz.Description,
             TimeLimitInMinutes = dto.TimeLimitInMinutes,
             QuestionCount = dto.QuestionsCount,
            }, "Quiz created successfully"); 
        }

        public async Task<ServiceResult<QuizUploadedResponseDto>> BulkImportQuizAsync(string title, int timeLimitInMinutes, Stream fileStream, string Description)
        {
            var parsedQuestions = _csvParser.ParseQuestionsFromStream(fileStream);
            if (!parsedQuestions.Any()) return ServiceResult<QuizUploadedResponseDto>.Failed("No valid questions found in the CSV.");


            var quizId = Guid.NewGuid();

            var questionsEntities = parsedQuestions.Select((q, index) => new Question
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                Text = q.Text,
                Order = index+1,
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
                QuestionCount = questionsEntities.Count,

                TimeLimit = TimeSpan.FromMinutes(timeLimitInMinutes),
                Questions = questionsEntities,
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            var data = new QuizUploadedResponseDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                Slug = quiz.Slug,
                QuestionCount = quiz.QuestionCount,
                QuizType = quiz.QuizType
            };

            return ServiceResult<QuizUploadedResponseDto>.Ok(data, "Quiz created succesfuly");
        }
    }
}
