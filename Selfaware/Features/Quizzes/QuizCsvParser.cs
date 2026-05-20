using Selfaware.Features.Quizzes.DTOs;
using System.Text.RegularExpressions;

namespace Selfaware.Features.Quizzes
{
    public class QuizCsvParser : IQuizCsvParser
    {
        public List<ParsedQuestionDto> ParseQuestionsFromStream(Stream fileStream)
        {
            var questions = new List<ParsedQuestionDto>();
            using var reader = new StreamReader(fileStream);
            var header = reader.ReadLine(); 

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var tokens = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                int? correctIndex = null;

                if (tokens.Length < 5) continue;

                var text = tokens[0].Trim('"');
                var options = new List<string>
        {
            tokens[1].Trim('"'),
            tokens[2].Trim('"'),
            tokens[3].Trim('"'),
            tokens[4].Trim('"')
        };

               
                if (tokens.Length >= 6 && int.TryParse(tokens[5].Trim(), out int parsedIndex))
                {
                    correctIndex = parsedIndex;
                }

                
                questions.Add(new ParsedQuestionDto(text, options, correctIndex));
            }

            return questions;
        }
    }
}
