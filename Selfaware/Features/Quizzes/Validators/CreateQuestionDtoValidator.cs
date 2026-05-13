using FluentValidation;
using Selfaware.Features.Quizzes.DTOs;

public class CreateQuestionDtoValidator : AbstractValidator<CreateQuestionDto>
{
    public CreateQuestionDtoValidator()
    {
        RuleFor(x => x.Text).NotEmpty().WithMessage("Question text is required.");
        RuleFor(x => x.QuestionType).NotEmpty();
        RuleForEach(x => x.OptionsJson).ChildRules(option =>
        {
            option.RuleFor(o => o.Text)
                .NotEmpty().WithMessage("Each option must have text.");
        });
    }
}
