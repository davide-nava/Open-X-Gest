using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Application.Features.TimeTracking.Mapping;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Enums;
using OpenX.Gest.Domain.Services;
using OpenX.Gest.Domain.ValueObjects;

namespace OpenX.Gest.Application.Features.TimeTracking.Commands.ClockOut;

/// <summary>
/// MediatR request handler for processing employee clock-out punches.
/// </summary>
public class ClockOutCommandHandler : IRequestHandler<ClockOutCommand, Result<TimeEntryDto>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IStringLocalizer<ValidationMessages> _localizer;
    private readonly SwissWorktimePolicy _policy;

    public ClockOutCommandHandler(
        ITimeEntryRepository timeEntryRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IStringLocalizer<ValidationMessages> localizer)
    {
        _timeEntryRepository = timeEntryRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _localizer = localizer;
        _policy = new SwissWorktimePolicy();
    }

    public async Task<Result<TimeEntryDto>> Handle(ClockOutCommand request, CancellationToken cancellationToken)
    {
        var activeEntry = await _timeEntryRepository.GetActiveEntryForEmployeeAsync(request.EmployeeId, cancellationToken);
        if (activeEntry == null)
        {
            return Result<TimeEntryDto>.Failure(Error.NotFound("TimeEntry.NotFound", _localizer["NoActiveShiftFound"]));
        }

        var nowUtc = _dateTimeProvider.UtcNow;

        GpsCoordinate? gps = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            gps = new GpsCoordinate(request.Latitude.Value, request.Longitude.Value, request.AccuracyMeters, nowUtc);
        }

        var clockOutResult = activeEntry.ClockOut(nowUtc, request.BreakDurationMinutes, gps);
        if (clockOutResult.IsFailure)
        {
            return Result<TimeEntryDto>.Failure(clockOutResult.Error);
        }

        // Evaluate daily maximum amplitude (Art. 10 LL / Art. 13 OLL 1: max 14h)
        var (amplitudeHours, amplitudeExceeded) = _policy.EvaluateDailyAmplitude(activeEntry.ClockInUtc, nowUtc);

        // Evaluate statutory mandatory breaks (Art. 15 LL)
        var workedHours = (nowUtc - activeEntry.ClockInUtc).TotalHours;
        var requiredBreakMinutes = _policy.CalculateStatutoryBreakMinutes(workedHours);
        var breakInsufficient = request.BreakDurationMinutes < requiredBreakMinutes;

        var violations = activeEntry.Violations;
        if (amplitudeExceeded)
        {
            violations |= ViolationType.DailyAmplitudeExceeded;
        }

        if (breakInsufficient)
        {
            violations |= ViolationType.InsufficientBreak;
        }

        activeEntry.SetComplianceMetrics(
            activeEntry.RestPeriodHoursBeforeShift,
            activeEntry.DailyRestPeriodViolated,
            amplitudeHours,
            amplitudeExceeded,
            violations);

        _timeEntryRepository.Update(activeEntry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TimeEntryDto>.Success(activeEntry.ToDto(_dateTimeProvider));
    }
}
