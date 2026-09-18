using FluentValidation;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.Features.Employees.Queries.GetEmployeeById;

/// <summary>
/// FluentValidation validator for GetEmployeeByIdQuery.
/// </summary>
public class GetEmployeeByIdQueryValidator : AbstractValidator<GetEmployeeByIdQuery>
{
    public GetEmployeeByIdQueryValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(localizer["EmployeeIdRequired"]);
    }
}
