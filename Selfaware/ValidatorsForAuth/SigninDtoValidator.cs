using FluentValidation;
using Selfaware.Models.DTOs;

public class SignInDtoValidator : AbstractValidator<SigninDto>
{
    public SignInDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}