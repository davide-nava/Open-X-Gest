using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.Auth.Commands;

/// <summary>
/// FluentValidation validator for LoginCommand.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["EmailRequired"])
            .EmailAddress()
            .WithMessage(localizer["InvalidEmail"]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(localizer["PasswordRequired"]);
    }
}
