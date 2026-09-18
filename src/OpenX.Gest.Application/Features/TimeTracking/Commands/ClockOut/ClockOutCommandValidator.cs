using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.TimeTracking.Commands.ClockOut;

/// <summary>
/// FluentValidation validator for ClockOutCommand.
/// </summary>
public class ClockOutCommandValidator : AbstractValidator<ClockOutCommand>
{
    public ClockOutCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage(localizer["EmployeeIdRequired"]);

        RuleFor(x => x.BreakDurationMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["BreakDurationNegative"]);
    }
}
