using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Shared.Helpers;
using Selfaware.Shared.Models;

namespace Selfaware.Shared.AI
{
    public class GPTClient : IAIClient
    {
        private readonly ChatClient _gptClient;
        private readonly GPTSettings _gptSettings;

        public GPTClient(IOptions<GPTSettings> gptSettings)
        {
            _gptSettings = gptSettings.Value;
            Console.WriteLine(_gptSettings.ApiKey);
            _gptClient = new ChatClient(model: "gpt-5.4-mini", apiKey: _gptSettings.ApiKey);
        }

        public async Task<ServiceResult<AiQuizResponseDto>> AskAiAsync(
            string systemPrompt,
            string userText
        )
        {
            try
            {
                Console.WriteLine("IM in AI CLIENT");
                userText = Regex.Replace(userText, @"\s+", " ").Trim();
                if (string.IsNullOrEmpty(userText))
                    throw new ArgumentNullException(nameof(userText), "Extracted text is empty!");
                if (string.IsNullOrEmpty(systemPrompt))
                    throw new ArgumentNullException(
                        nameof(systemPrompt),
                        "System prompt is empty!"
                    );

                List<ChatMessage> messages = new()
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userText),
                };

                string rawSchemaJson = QuizSchemaProvider.GetOpenAiJsonSchemaString();

                ChatCompletionOptions options = new()
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "quiz_generation",
                        jsonSchema: BinaryData.FromString(
                            QuizSchemaProvider.GetOpenAiJsonSchemaString()
                        ),
                        jsonSchemaIsStrict: true
                    ),
                    Temperature = 0.2f,
                };

                ChatCompletion completion = await _gptClient.CompleteChatAsync(messages, options);

                if (completion == null || completion.Content.Count == 0)
                {
                    return ServiceResult<AiQuizResponseDto>.Failed(
                        "OpenAI failed to return any completion choices."
                    );
                }

                var jsonResponse = completion.Content[0].Text;

                Console.WriteLine($"RAW AI response {jsonResponse}");
                var questions = JsonSerializer.Deserialize<AiQuizResponseDto>(
                    jsonResponse,
                    JsonSettings.Options
                );

                if (questions == null)
                {
                    return ServiceResult<AiQuizResponseDto>.Failed(
                        "Failed to parse the OpenAI JSON response."
                    );
                }

                Console.WriteLine($"Deserialazied Generated Quiz {questions.Questions}");
                return ServiceResult<AiQuizResponseDto>.Ok(
                    questions,
                    "Quiz generated successfully via OpenAI"
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<AiQuizResponseDto>.Failed(
                    $"System crashed during OpenAI generation: {ex.Message}"
                );
            }
        }
    }
}
