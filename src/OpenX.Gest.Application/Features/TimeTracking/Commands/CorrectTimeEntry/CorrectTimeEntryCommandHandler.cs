using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Application.Features.TimeTracking.Mapping;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Enums;
using OpenX.Gest.Domain.Services;

namespace OpenX.Gest.Application.Features.TimeTracking.Commands.CorrectTimeEntry;

/// <summary>
/// MediatR request handler for processing retroactive time entry corrections and compliance recalculation.
/// </summary>
public class CorrectTimeEntryCommandHandler : IRequestHandler<CorrectTimeEntryCommand, Result<TimeEntryDto>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStringLocalizer<ValidationMessages> _localizer;
    private readonly SwissWorktimePolicy _policy;

    public CorrectTimeEntryCommandHandler(
        ITimeEntryRepository timeEntryRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService,
        IStringLocalizer<ValidationMessages> localizer)
    {
        _timeEntryRepository = timeEntryRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
        _localizer = localizer;
        _policy = new SwissWorktimePolicy();
    }

    public async Task<Result<TimeEntryDto>> Handle(CorrectTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _timeEntryRepository.GetByIdAsync(request.TimeEntryId, cancellationToken);
        if (entry == null)
        {
            return Result<TimeEntryDto>.Failure(Error.NotFound("TimeEntry.NotFound", _localizer["TimeEntryNotFound"]));
        }

        // Apply domain correction and generate immutable audit log record
        var operatorId = request.OperatorId != Guid.Empty
            ? request.OperatorId
            : _currentUserService.UserId ?? Guid.Empty;

        var correctionResult = entry.ApplyCorrection(
            operatorId,
            request.NewClockInUtc,
            request.NewClockOutUtc,
            request.NewBreakMinutes,
            request.MandatoryReason);

        if (correctionResult.IsFailure)
        {
            return Result<TimeEntryDto>.Failure(correctionResult.Error);
        }

        // Recalculate Swiss labor compliance violations
        if (request.NewClockOutUtc.HasValue)
        {
            var (ampHours, ampExceeded) = _policy.EvaluateDailyAmplitude(request.NewClockInUtc, request.NewClockOutUtc.Value);
            var workedHours = (request.NewClockOutUtc.Value - request.NewClockInUtc).TotalHours;
            var requiredBreak = _policy.CalculateStatutoryBreakMinutes(workedHours);
            var breakInsufficient = request.NewBreakMinutes < requiredBreak;

            var violations = ViolationType.None;
            if (entry.DailyRestPeriodViolated)
            {
                violations |= ViolationType.DailyRestPeriodViolated;
            }

            if (ampExceeded)
            {
                violations |= ViolationType.DailyAmplitudeExceeded;
            }

            if (breakInsufficient)
            {
                violations |= ViolationType.InsufficientBreak;
            }

            entry.SetComplianceMetrics(
                entry.RestPeriodHoursBeforeShift,
                entry.DailyRestPeriodViolated,
                ampHours,
                ampExceeded,
                violations);
        }

        await _timeEntryRepository.AddCorrectionAuditAsync(correctionResult.Value, cancellationToken);
        _timeEntryRepository.Update(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TimeEntryDto>.Success(entry.ToDto(_dateTimeProvider));
    }
}
