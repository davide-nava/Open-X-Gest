using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Domain.Entities;

namespace OpenX.Gest.Application.Features.TimeTracking.Mapping;

public static class TimeTrackingMapper
{
    public static TimeEntryDto ToDto(this TimeEntry entry, IDateTimeProvider dateTimeProvider)
    {
        var endUtc = entry.ClockOutUtc ?? dateTimeProvider.UtcNow;
        var totalMinutes = (endUtc - entry.ClockInUtc).TotalMinutes;
        var netMinutes = Math.Max(0, totalMinutes - entry.BreakDurationMinutes);
        var netWorkedHours = Math.Round(netMinutes / 60.0, 2);

        return new TimeEntryDto(
            entry.Id,
            entry.EmployeeId,
            entry.ClockInUtc,
            entry.ClockOutUtc,
            dateTimeProvider.ToSwissTime(entry.ClockInUtc),
            entry.ClockOutUtc.HasValue ? dateTimeProvider.ToSwissTime(entry.ClockOutUtc.Value) : null,
            entry.BreakDurationMinutes,
            netWorkedHours,
            entry.PunctualClockInGps != null,
            entry.PunctualClockOutGps != null,
            entry.RestPeriodHoursBeforeShift,
            entry.DailyRestPeriodViolated,
            entry.DailyAmplitudeHours,
            entry.DailyAmplitudeExceeded,
            entry.Notes,
            entry.Status,
            entry.Violations,
            entry.AuditTrail.Select(a => new TimeCorrectionAuditDto(
                a.Id,
                a.OperatorId,
                a.TimestampUtc,
                a.PreCorrectionClockInUtc,
                a.PreCorrectionClockOutUtc,
                a.PreCorrectionBreakMinutes,
                a.PostCorrectionClockInUtc,
                a.PostCorrectionClockOutUtc,
                a.PostCorrectionBreakMinutes,
                a.MandatoryReason
            )).ToList()
        );
    }
}
