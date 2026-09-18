using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeRegime;

/// <summary>
/// FluentValidation validator for UpdateEmployeeRegimeCommand.
/// </summary>
public class UpdateEmployeeRegimeCommandValidator : AbstractValidator<UpdateEmployeeRegimeCommand>
{
    public UpdateEmployeeRegimeCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage(localizer["EmployeeIdRequired"]);

        RuleFor(x => x.Regime)
            .IsInEnum()
            .WithMessage(localizer["GeneralError"]);
    }
}
