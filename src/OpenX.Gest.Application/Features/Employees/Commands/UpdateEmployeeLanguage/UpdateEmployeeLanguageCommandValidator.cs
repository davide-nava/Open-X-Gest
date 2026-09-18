using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeLanguage;

/// <summary>
/// FluentValidation validator for UpdateEmployeeLanguageCommand.
/// </summary>
public class UpdateEmployeeLanguageCommandValidator : AbstractValidator<UpdateEmployeeLanguageCommand>
{
    public UpdateEmployeeLanguageCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage(localizer["EmployeeIdRequired"]);

        RuleFor(x => x.Language)
            .IsInEnum()
            .WithMessage(localizer["GeneralError"]);
    }
}
