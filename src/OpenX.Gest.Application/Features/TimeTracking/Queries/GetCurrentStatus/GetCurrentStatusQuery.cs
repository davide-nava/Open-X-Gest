using MediatR;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.TimeTracking.Queries.GetCurrentStatus;

/// <summary>
/// Query to obtain the live, real-time clock-in attendance status of an employee.
/// </summary>
public record GetCurrentStatusQuery(Guid EmployeeId) : IRequest<Result<CurrentTimeStatusDto>>;
