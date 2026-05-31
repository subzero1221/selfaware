using Selfaware.Features.Quizzes.DTOs;

namespace Selfaware.Features.Quizzes.Parsers
{
    public interface IQuizCsvParser
    {
        List<ParsedQuestionDto> ParseQuestionsFromStream(Stream fileStream);
    }
}
