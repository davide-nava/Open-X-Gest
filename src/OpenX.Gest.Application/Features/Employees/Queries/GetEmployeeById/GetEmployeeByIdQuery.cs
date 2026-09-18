using MediatR;
using OpenX.Gest.Application.Features.Employees.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.Employees.Queries.GetEmployeeById;

/// <summary>
/// Query to retrieve a single employee by their unique identifier.
/// </summary>
public record GetEmployeeByIdQuery(Guid Id) : IRequest<Result<EmployeeDto>>;
