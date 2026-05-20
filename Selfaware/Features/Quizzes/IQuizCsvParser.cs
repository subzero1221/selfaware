using Selfaware.Features.Quizzes.DTOs;

namespace Selfaware.Features.Quizzes
{
    public interface IQuizCsvParser
    {
        List<ParsedQuestionDto> ParseQuestionsFromStream(Stream fileStream);
    }
}
