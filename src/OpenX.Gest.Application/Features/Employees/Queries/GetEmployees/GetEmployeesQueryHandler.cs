using MediatR;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.Employees.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.Employees.Queries.GetEmployees;

/// <summary>
/// MediatR request handler for retrieving all employees.
/// </summary>
public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<List<EmployeeDto>>>
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<List<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var dtos = employees.Select(EmployeeDto.FromEntity).ToList();
        return Result<List<EmployeeDto>>.Success(dtos);
    }
}
