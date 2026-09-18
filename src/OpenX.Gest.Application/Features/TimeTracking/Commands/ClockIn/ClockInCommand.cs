using MediatR;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.TimeTracking.Commands.ClockIn;

/// <summary>
/// Command to register an employee's clock-in punch with optional punctual GPS coordinates.
/// </summary>
public record ClockInCommand(
    Guid EmployeeId,
    double? Latitude = null,
    double? Longitude = null,
    double? AccuracyMeters = null,
    string? Notes = null
) : IRequest<Result<TimeEntryDto>>;
