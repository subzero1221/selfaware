using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;
using Selfaware.Shared.Helpers;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Shared.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using Selfaware.Features.Quizzes.Enums;

namespace Selfaware.Shared.AI
{
    public class GeminiClient : IAIClient
    {
        private readonly Client _genAiClient;
        private readonly AiSettings _geminiSettings;
  
        public GeminiClient(IOptions<AiSettings> geminiSettings)
        {
            _geminiSettings = geminiSettings.Value;
            _genAiClient = new Client(apiKey: _geminiSettings.ApiKey);
        }
        public async Task<ServiceResult<AiQuizResponseDto>> AskAiAsync(string systemPrompt, string userText)
        {
            try
            {
                userText = Regex.Replace(userText, @"\s+", " ").Trim();
                if (string.IsNullOrEmpty(userText)) throw new ArgumentNullException(nameof(userText), "Extracted text is empty!");
                if (string.IsNullOrEmpty(systemPrompt)) throw new ArgumentNullException(nameof(systemPrompt), "System prompt is empty!");


                var config = new GenerateContentConfig
                {

                    SystemInstruction = new Content
                    {
                        Parts = new List<Part> { new Part { Text = systemPrompt } }
                    },
                    ResponseMimeType = "application/json",
                    ResponseSchema = QuizSchemaProvider.GetQuizSchema(),
                    MaxOutputTokens = 8192,
                    Temperature = 0.2f
                };


                var response = await _genAiClient.Models.GenerateContentAsync(
                    model: "gemini-2.5-flash",
                    contents: userText,
                    config: config
                );



                if (response == null || response.Candidates == null || response.Candidates.Count == 0)
                {
                    return ServiceResult<AiQuizResponseDto>.Failed("AI API failed to return any candidates.");
                }

                var firstCandidate = response.Candidates[0];
                if (firstCandidate.Content == null || firstCandidate.Content.Parts == null || firstCandidate.Content.Parts.Count == 0)
                {
                    return ServiceResult<AiQuizResponseDto>.Failed($"AI returned empty content. Finish Reason: {firstCandidate.FinishReason}");
                }

                string jsonResponse = firstCandidate.Content.Parts[0].Text;
                Console.WriteLine($"here comes quiz generated questions:{jsonResponse}");
               
                var questions = JsonSerializer.Deserialize<AiQuizResponseDto>(jsonResponse, JsonSettings.Options);

                Console.WriteLine($"After JSonded questions:{jsonResponse}");
                if (questions == null)  
                { 
                    return ServiceResult<AiQuizResponseDto>.Failed("Failed to parse the AI JSON.");
                }

                return ServiceResult<AiQuizResponseDto>.Ok(questions, "Quiz generated successfully");
            }
            catch (Exception ex)
            {
                
                return ServiceResult<AiQuizResponseDto>.Failed($"System crashed during AI generation: {ex.Message}");
            }
        }
    }
}