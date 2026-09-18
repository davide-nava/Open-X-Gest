using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.Features.TimeTracking.DTOs;

public record TimeEntryDto(
    Guid Id,
    Guid EmployeeId,
    DateTime ClockInUtc,
    DateTime? ClockOutUtc,
    DateTime ClockInSwiss,
    DateTime? ClockOutSwiss,
    int BreakDurationMinutes,
    double NetWorkedHours,
    bool HasClockInGps,
    bool HasClockOutGps,
    double? RestPeriodHoursBeforeShift,
    bool DailyRestPeriodViolated,
    double? DailyAmplitudeHours,
    bool DailyAmplitudeExceeded,
    string? Notes,
    TimeEntryStatus Status,
    ViolationType Violations,
    List<TimeCorrectionAuditDto> AuditTrail
);

public record TimeCorrectionAuditDto(
    Guid Id,
    Guid OperatorId,
    DateTime TimestampUtc,
    DateTime PreClockInUtc,
    DateTime? PreClockOutUtc,
    int PreBreakMinutes,
    DateTime PostClockInUtc,
    DateTime? PostClockOutUtc,
    int PostBreakMinutes,
    string MandatoryReason
);

public record CurrentTimeStatusDto(
    Guid EmployeeId,
    bool IsClockedIn,
    Guid? ActiveEntryId,
    DateTime? ClockInUtc,
    DateTime? ClockInSwiss,
    double ElapsedWorkedHoursToday,
    int SuggestedStatutoryBreakMinutes,
    Oll1Regime Oll1Regime,
    string EmployeeFullName
);

public record DaySummaryDto(
    DateTime Date,
    string DateFormatted,
    double GrossHours,
    int TotalBreakMinutes,
    double NetWorkedHours,
    double OrdinaryHours,
    double SupplementaryHours, // Überstunden
    double StatutoryOvertimeHours, // Überzeit
    double NightHours,
    double SundayHours,
    bool HasRestViolation,
    bool HasAmplitudeViolation,
    List<TimeEntryDto> Entries
);

public record TimesheetDto(
    Guid EmployeeId,
    string EmployeeName,
    Oll1Regime Oll1Regime,
    decimal ContractualWeeklyHours,
    StatutoryWeeklyLimit StatutoryWeeklyLimit,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    double TotalNetWorkedHours,
    double TotalOrdinaryHours,
    double TotalSupplementaryHours, // Überstunden (CO art. 321c)
    double TotalStatutoryOvertimeHours, // Überzeit (LL art. 12/13)
    double TotalNightHours,
    double TotalSundayHours,
    int TotalRestViolations,
    int TotalAmplitudeViolations,
    List<DaySummaryDto> Days
);

public record SecoExportDto(
    string FileName,
    string ContentType,
    byte[] FileBytes
);
