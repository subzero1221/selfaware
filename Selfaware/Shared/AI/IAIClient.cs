using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Shared.AI
{
    public interface IAIClient
    {
        Task<ServiceResult<AiQuizResponseDto>> AskAiAsync(string systemPrompt, string userText);
    }
}
