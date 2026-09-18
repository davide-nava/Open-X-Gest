using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.Employees.DTOs;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.Employees.Queries.GetEmployeeById;

/// <summary>
/// MediatR request handler for retrieving an employee by Id.
/// </summary>
public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IStringLocalizer<ValidationMessages> _localizer;

    public GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository, IStringLocalizer<ValidationMessages> localizer)
    {
        _employeeRepository = employeeRepository;
        _localizer = localizer;
    }

    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee == null)
        {
            return Result<EmployeeDto>.Failure(Error.NotFound("Employee.NotFound", _localizer["EmployeeNotFound"]));
        }

        return Result<EmployeeDto>.Success(EmployeeDto.FromEntity(employee));
    }
}
