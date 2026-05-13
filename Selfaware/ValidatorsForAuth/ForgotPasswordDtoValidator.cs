using FluentValidation;
using Selfaware.Models.DTOs;

namespace Selfaware.ValidatorsForAuth
{
    public class ForgotPasswordValidator:AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordValidator(){
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Please enter a valid email address.");
        }
    }
}
