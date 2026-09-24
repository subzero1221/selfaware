using FluentValidation;
using Selfaware.Features.Survey.SurveySession.Dtos;

namespace Selfaware.Features.Survey.SurveySession.Validators
{
    public class StartSurveySessionValidator:AbstractValidator<StartSurveySessionDto>
    {
        public  StartSurveySessionValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Email)
                 .EmailAddress()
                 .WithMessage("Please enter a valid email address.")
                 .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
