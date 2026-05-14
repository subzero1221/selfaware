using FluentValidation;
using Selfaware.Features.Auth.DTOs;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto> 
{ 
    public ChangePasswordValidator()
    {

        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current Password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters.");

        RuleFor(x => x.ConfirmPassword)
           .NotEmpty().WithMessage("Please confirm your password.")
           .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

