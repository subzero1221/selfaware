

namespace Selfaware.Features.Quizzes.DTOs
{
    public class CreateQuestionDto
    {
       
        public string Text { get; set; } = string.Empty;

        public int Order { get; set; }
        //fronts sheveci trakshi//
       
        public string? QuestionType { get; set; }


        public List<OptionDto> OptionsJson { get; set; }
    }
}
