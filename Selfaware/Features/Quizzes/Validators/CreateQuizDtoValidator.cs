using FluentValidation;
using Selfaware.Features.Quizzes.DTOs;

public class CreateQuizDtoValidator : AbstractValidator<CreateQuizDto>
{
    public CreateQuizDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Questions)
            .Must(q => q.Count <= 100)
            .WithMessage("A quiz cannot have more than 100 questions.");
        RuleForEach(x => x.Questions).Cascade(CascadeMode.Stop).SetValidator(new CreateQuestionDtoValidator()); 
    }
}
