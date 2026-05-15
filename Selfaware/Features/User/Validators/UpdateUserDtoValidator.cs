using FluentValidation;
using Selfaware.Features.User.DTOs;

namespace Selfaware.Shared.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.DisplayName)
            .MaximumLength(20).WithMessage("Display Name cannot exceed 50 characters.")
            .When(x => x.DisplayName != null);

        RuleFor(x => x.Bio)
            .MaximumLength(200).WithMessage("Bio is too long (max 200 characters).")
            .When(x => x.Bio != null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email is required.")
            .When(x => x.Email != null);
    }
}