using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.TimeTracking.Queries.ExportSecoReport;

/// <summary>
/// FluentValidation validator for ExportSecoReportQuery.
/// </summary>
public class ExportSecoReportQueryValidator : AbstractValidator<ExportSecoReportQuery>
{
    public ExportSecoReportQueryValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage(localizer["EmployeeIdRequired"]);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage(localizer["GeneralError"]);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage(localizer["GeneralError"]);
    }
}
