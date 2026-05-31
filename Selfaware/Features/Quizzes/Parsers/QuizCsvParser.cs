using Selfaware.Features.Quizzes.DTOs;
using System.Text.RegularExpressions;

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
