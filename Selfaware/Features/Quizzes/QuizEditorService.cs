using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Enums;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.Cloudinary;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Quizzes
{
    public class QuizEditorService : IQuizEditorService
    {
        public readonly AppDbContext _context;
        public readonly IImageService _cloudinaryService;

        public QuizEditorService(AppDbContext context, IImageService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;

        }

        public async Task<ServiceResult<Guid>> EditSettingsAsync(
            Guid quizId,
            string userId,
            EditQuizSettingsDto dto
        )
        {
            var quiz = await _context
                .Quizzes.Where(quiz => quiz.Id == quizId && quiz.CreatedById == userId)
                .FirstOrDefaultAsync();
            if (quiz == null)
            {
               
                return ServiceResult<Guid>.Failed("Quiz not found or access denied");
            }

           
            switch (dto.Field)
            {
                case SettingsField.Title:
                    quiz.Title = dto.Value;
                    break;

                case SettingsField.Description:
                    quiz.Description = dto.Value;
                    break;

                case SettingsField.QuizType:
                    quiz.QuizType = dto.Type?? QuizType.Knowledge;
                    break;

                case SettingsField.TimeLimit:
                    if (int.TryParse(dto.Value, out var timeInt))
                    {
                        quiz.TimeLimit = timeInt;
                    }
                    else
                    {
                        return ServiceResult<Guid>.Failed(
                            "Invalid time limit format. Must be an integer."
                        );
                    }
                    break;

                default:
                    return ServiceResult<Guid>.Failed("Unsupported settings field.");
            }

            await _context.SaveChangesAsync();

            return ServiceResult<Guid>.Ok(quizId, "Quiz settings edit success");
        }

        public async Task<ServiceResult<Guid>> EditQuestionAsync(
            Guid quizId,
            Guid questionId,
            string userId,
            EditQuestionDto dto
        )
        {
            var question = await _context
                .Questions.Include(q => q.Options)
                .FirstOrDefaultAsync(q =>
                    q.QuizId == quizId && q.Id == questionId && q.Quiz.CreatedById == userId
                );
            if (question == null)
            {
                return ServiceResult<Guid>.Failed("Question not found or access denied");
            }

            string? currentImagePublicId = question.ImagePublicId;
            if(!string.IsNullOrWhiteSpace(currentImagePublicId) && currentImagePublicId != dto.ImagePublicId)
            {
                await _cloudinaryService.DeleteImageAsync(currentImagePublicId);

            }

            question.Text = dto.Text;
            foreach (var dtoOpt in dto.Options)
            {
                var existingOpt = question.Options.FirstOrDefault(o => o.Id == dtoOpt.Id);

                if (existingOpt != null)
                {
                    existingOpt.Text = dtoOpt.Text;
                    existingOpt.Score = dtoOpt.Score;
                }
            }
            question.ImageUrl = dto.ImageUrl;
            question.ImagePublicId = dto.ImagePublicId;

            

            await _context.SaveChangesAsync();

            

                return ServiceResult<Guid>.Ok(questionId, "Question update success");
        }

        public async Task<ServiceResult<Guid>> DeleteQuestionAsync(
            Guid quizId,
            Guid questionId,
            string userId
        )
        {
            var question = await _context.Questions
                 .Include(q => q.Quiz)
                   .FirstOrDefaultAsync(q =>
                     q.Id == questionId
                     && q.QuizId == quizId
                     && q.Quiz.CreatedById == userId
                               );

            if (question == null)
            {
                return ServiceResult<Guid>.Failed("Question not found or access denied");
            }

           
            if (!string.IsNullOrEmpty(question.ImagePublicId))
            {
               
                await _cloudinaryService.DeleteImageAsync(question.ImagePublicId);
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();


            return ServiceResult<Guid>.Ok(questionId, "Question delete success");
        }
    }
}
