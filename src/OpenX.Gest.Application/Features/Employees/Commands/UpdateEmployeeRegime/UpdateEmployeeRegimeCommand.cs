using MediatR;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeRegime;

/// <summary>
/// Command to update the Swiss OLL 1 working time recording regime for an employee.
/// </summary>
public record UpdateEmployeeRegimeCommand(Guid EmployeeId, Oll1Regime Regime) : IRequest<Result>;
