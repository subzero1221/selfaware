using Selfaware.Features.Quizzes.DTOs;

namespace Selfaware.Features.Survey.DTOs
{
    public record SurveyDto
    (
        Guid SurveyId,
        QuizForSurveyDto Quiz,
        SurveyStatus Status 
        );


    
    public record QuizForSurveyDto
    (
        Guid QuizId,
        QuizType QuizType,
        QuizStatus QuizStatus,
        List<QuestionDto> Questions,
        int QuestionCount,
        string Description = null,
        string Title = null
    );

    //enums
    public enum SurveyStatus{
        Answering = 0,
        ShowingResults = 1,
        }
}
