using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Selfaware.Data;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Models.Entities;
using System.Text.Json;


namespace Selfaware.Features.Quizzes
{
    public class QuizService : IQuizService
    {
       

        private readonly AppDbContext _context; 

        public QuizService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateQuizAsync(CreateQuizDto model)
        {
            bool exists = await _context.Quizzes.AnyAsync(q => q.Title == model.Title);
            if (exists) throw new ArgumentException("A quiz with this title already exists.");

            var quiz = new Quiz
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                Questions = model.Questions.Select((q, index) => new Question
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

            return quiz.Id; 

        }
    }
}
