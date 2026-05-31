using FluentValidation;
using Selfaware.Features.Quizzes.DTOs;

public class CreateQuestionDtoValidator : AbstractValidator<CreateQuestionDto>
{
    public CreateQuestionDtoValidator()
    {
        RuleFor(x => x.Text).NotEmpty().WithMessage("Question text is required.");
        RuleFor(x => x.QuestionType).NotEmpty();      
    }
}
