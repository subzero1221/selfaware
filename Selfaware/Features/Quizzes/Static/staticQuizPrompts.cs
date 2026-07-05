namespace Selfaware.Features.Quizzes.Static
{
    namespace Selfaware.Features.Quizzes
    {
        public static class QuizPrompts
        {
            public const string ExtractExistingQuiz =
                @"You are an expert educational content parser. Your task is to extract a quiz from the provided text and Generate 4 options per question. output it as a valid JSON object.CRITICAL: Do NOT output newline characters (\n) or excessive whitespace inside the JSON string values. Format all text as clean, single-line strings.";
            public const string GenerateNewQuizFromText =
                @"You are a test generator. Your task is to generate quiz based on the provided learning material and 4 answer options per question. CRITICAL CONTENT RULES:
               Keep all 'text' fields for questions short and concise (under 30 words).
               Keep all 'text' fields for options extremely short (under 5 words).
               Do not include introductory text, explanations, or 'The correct answer is...'.
               Only return the raw JSON object.";
        }
    }
}
