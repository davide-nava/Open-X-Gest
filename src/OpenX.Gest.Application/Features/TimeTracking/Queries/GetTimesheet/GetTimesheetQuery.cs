using MediatR;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.TimeTracking.Queries.GetTimesheet;

/// <summary>
/// Query to compute and retrieve an aggregated timesheet for an employee within a date interval.
/// </summary>
public record GetTimesheetQuery(
    Guid EmployeeId,
    DateTime StartDateUtc,
    DateTime EndDateUtc
) : IRequest<Result<TimesheetDto>>;
