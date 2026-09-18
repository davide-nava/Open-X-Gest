using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Application.Features.TimeTracking.Mapping;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;
using OpenX.Gest.Domain.Services;
using OpenX.Gest.Domain.ValueObjects;

namespace OpenX.Gest.Application.Features.TimeTracking.Commands.ClockIn;

/// <summary>
/// MediatR request handler for processing employee clock-in punches.
/// </summary>
public class ClockInCommandHandler : IRequestHandler<ClockInCommand, Result<TimeEntryDto>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IStringLocalizer<ValidationMessages> _localizer;
    private readonly SwissWorktimePolicy _policy;

    public ClockInCommandHandler(
        ITimeEntryRepository timeEntryRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IStringLocalizer<ValidationMessages> localizer)
    {
        _timeEntryRepository = timeEntryRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _localizer = localizer;
        _policy = new SwissWorktimePolicy();
    }

    public async Task<Result<TimeEntryDto>> Handle(ClockInCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result<TimeEntryDto>.Failure(Error.NotFound("Employee.NotFound", _localizer["EmployeeNotFound"]));
        }

        var activeEntry = await _timeEntryRepository.GetActiveEntryForEmployeeAsync(request.EmployeeId, cancellationToken);
        if (activeEntry != null)
        {
            return Result<TimeEntryDto>.Failure(Error.Conflict("TimeEntry.ActiveExists", _localizer["ActiveShiftAlreadyExists"]));
        }

        var nowUtc = _dateTimeProvider.UtcNow;

        GpsCoordinate? gps = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            gps = new GpsCoordinate(request.Latitude.Value, request.Longitude.Value, request.AccuracyMeters, nowUtc);
        }

        var entry = new TimeEntry(Guid.NewGuid(), employee.Id, nowUtc, gps, request.Notes);

        // Swiss legal check: 11 consecutive hours daily rest period (Art. 15a LL / Art. 19 OLL 1)
        var prevEntry = await _timeEntryRepository.GetPreviousEntryBeforeAsync(request.EmployeeId, nowUtc, cancellationToken);
        if (prevEntry?.ClockOutUtc != null)
        {
            var (restHours, restViolated) = _policy.EvaluateDailyRestPeriod(prevEntry.ClockOutUtc.Value, nowUtc);
            var violations = restViolated ? ViolationType.DailyRestPeriodViolated : ViolationType.None;
            entry.SetComplianceMetrics(restHours, restViolated, null, false, violations);
        }

        await _timeEntryRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TimeEntryDto>.Success(entry.ToDto(_dateTimeProvider));
    }
}
