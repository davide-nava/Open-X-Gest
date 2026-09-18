using MediatR;
using OpenX.Gest.Application.Features.Employees.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.Employees.Queries.GetEmployees;

/// <summary>
/// Query to retrieve all registered employees and their contractual profiles.
/// </summary>
public record GetEmployeesQuery() : IRequest<Result<List<EmployeeDto>>>;
