using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.TimeTracking.Commands.ClockIn;

/// <summary>
/// FluentValidation validator for ClockInCommand.
/// </summary>
public class ClockInCommandValidator : AbstractValidator<ClockInCommand>
{
    public ClockInCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage(localizer["EmployeeIdRequired"]);
    }
}
