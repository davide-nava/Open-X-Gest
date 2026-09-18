using MediatR;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.TimeTracking.Commands.ClockOut;

/// <summary>
/// Command to register an employee's clock-out punch with break duration and optional punctual GPS coordinates.
/// </summary>
public record ClockOutCommand(
    Guid EmployeeId,
    int BreakDurationMinutes = 0,
    double? Latitude = null,
    double? Longitude = null,
    double? AccuracyMeters = null
) : IRequest<Result<TimeEntryDto>>;
