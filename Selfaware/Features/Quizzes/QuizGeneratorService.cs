using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.DTOs.Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Features.Quizzes.Enums;
using Selfaware.Features.Quizzes.Static.Selfaware.Features.Quizzes;
using Selfaware.Infrastructure.Data;
using Selfaware.Shared.AI;
using Selfaware.Shared.DocumentExtraction;
using Selfaware.Shared.Models;



namespace Selfaware.Features.Quizzes
{
    public class QuizGeneratorService:IQuizGeneratorService
    {
        public readonly ITextExtractor _textExtractor;
        public readonly IAIClient _aiClient;
        public readonly AppDbContext _context;

        public QuizGeneratorService(ITextExtractor textExtractor, IAIClient aiclient, AppDbContext context)
        {
            _textExtractor = textExtractor;
            _aiClient = aiclient;
            _context = context;
        }

        public async Task<ServiceResult<Guid>> ExtractExistingQuizAsync(ExtractQuizRequestDto dto, string currentUserId, CancellationToken cancellationToken)
        {
            using var stream = dto.File.OpenReadStream();
            stream.Position = 0;
            string fileName = dto.File.FileName;
            string systemPrompt = QuizPrompts.GenerateNewQuizFromText;

            if (stream == null) return ServiceResult<Guid>.Failed("No file provided.");

            string extension = Path.GetExtension(fileName).ToLower();
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            stream.Position = 0;

            ITextExtractor? extractor = (buffer[0], buffer[1], buffer[2], buffer[3]) switch
            {
                (0x25, 0x50, 0x44, 0x46) => new PdfExtractor(),
                (0x50, 0x4B, 0x03, 0x04) => new DocExtractor(),
                _ => null
            };

            if (extractor == null)
                return ServiceResult<Guid>.Failed("Invalid file format.");

            string rawText = extractor.ExtractText(stream);

            var aiResponse = await _aiClient.AskAiAsync(rawText, systemPrompt);

            if (!aiResponse.Success || aiResponse.Data == null)
            {
                return ServiceResult<Guid>.Failed(aiResponse.Message);
            } 

            var questions = aiResponse.Data;

            var generatedQuiz = new Quiz
            {
                Id = Guid.NewGuid(),
                Title = "",
                Description = "",
                CreatedById = currentUserId,
                Slug = $"draft-{Guid.NewGuid()}",
                QuizStatus = 0,
                TimeLimit = TimeSpan.FromMinutes(30),
                Questions = questions.Questions.Select((q, index) => new Question
                {
                    Id = Guid.NewGuid(),
                    Text = q.Text,
                    Order = index + 1,
                    Type = QuestionType.MultipleChoice,

                    Options = q.Options.Select(o => new Option
                    {
                        Id = Guid.NewGuid(),
                        Text = o.Text,
                        Score = o.Score
                    }).ToList()
                }).ToList()
            }; 

             _context.Quizzes.Add(generatedQuiz);
            await _context.SaveChangesAsync(cancellationToken);

            var draftQuizId = generatedQuiz.Id;
                

            return ServiceResult<Guid>.Ok(draftQuizId, aiResponse.Message);
        }

    }
}
