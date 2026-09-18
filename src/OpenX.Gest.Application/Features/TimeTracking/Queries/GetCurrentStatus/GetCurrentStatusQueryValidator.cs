using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.TimeTracking.Queries.GetCurrentStatus;

/// <summary>
/// FluentValidation validator for GetCurrentStatusQuery.
/// </summary>
public class GetCurrentStatusQueryValidator : AbstractValidator<GetCurrentStatusQuery>
{
    public GetCurrentStatusQueryValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage(localizer["EmployeeIdRequired"]);
    }
}
