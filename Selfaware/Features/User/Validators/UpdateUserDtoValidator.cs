using FluentValidation;
using FluentValidation.Validators;
using Selfaware.Features.User.DTOs;

namespace Selfaware.Shared.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.DisplayName)
             .MaximumLength(20)
             .WithMessage("Display Name cannot exceed 20 characters.") 
             .When(x => x.DisplayName != null);

        RuleFor(x => x.Bio)
            .MaximumLength(200)
            .WithMessage("Bio is too long (max 200 characters).")
            .When(x => x.Bio != null);

        
        RuleFor(x => x.Email)
            .EmailAddress(EmailValidationMode.AspNetCoreCompatible)
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            .WithMessage("A valid email with a domain extension (e.g. name@domain.com) is required.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
