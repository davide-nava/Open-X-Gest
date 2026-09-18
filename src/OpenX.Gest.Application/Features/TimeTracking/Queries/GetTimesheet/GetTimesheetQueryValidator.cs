using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.TimeTracking.Queries.GetTimesheet;

/// <summary>
/// FluentValidation validator for GetTimesheetQuery.
/// </summary>
public class GetTimesheetQueryValidator : AbstractValidator<GetTimesheetQuery>
{
    public GetTimesheetQueryValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage(localizer["EmployeeIdRequired"]);

        RuleFor(x => x.EndDateUtc)
            .GreaterThanOrEqualTo(x => x.StartDateUtc)
            .WithMessage(localizer["GeneralError"]);
    }
}
