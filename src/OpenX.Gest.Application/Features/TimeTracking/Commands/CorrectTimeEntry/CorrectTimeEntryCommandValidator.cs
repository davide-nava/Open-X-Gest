using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.TimeTracking.Commands.CorrectTimeEntry;

/// <summary>
/// FluentValidation validator for CorrectTimeEntryCommand.
/// </summary>
public class CorrectTimeEntryCommandValidator : AbstractValidator<CorrectTimeEntryCommand>
{
    public CorrectTimeEntryCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.TimeEntryId)
            .NotEmpty();

        RuleFor(x => x.MandatoryReason)
            .NotEmpty()
            .WithMessage(localizer["MandatoryReasonRequired"])
            .MinimumLength(5)
            .WithMessage(localizer["MandatoryReasonRequired"]);

        RuleFor(x => x.NewBreakMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["BreakDurationNegative"]);
    }
}
