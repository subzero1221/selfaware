using System.Text.RegularExpressions;
using Selfaware.Features.Quizzes.DTOs;

namespace Selfaware.Features.Quizzes.Parsers
{
    public class QuizCsvParser : IQuizCsvParser
    {
        public List<ParsedQuestionDto> ParseQuestionsFromStream(Stream fileStream)
        {
            var questions = new List<ParsedQuestionDto>();

            return questions;
        }
    }
}
