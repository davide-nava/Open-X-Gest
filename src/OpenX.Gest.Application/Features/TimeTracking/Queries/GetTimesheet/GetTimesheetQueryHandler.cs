using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Application.Features.TimeTracking.Mapping;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Services;

namespace OpenX.Gest.Application.Features.TimeTracking.Queries.GetTimesheet;

/// <summary>
/// MediatR request handler for computing and projecting employee timesheets.
/// </summary>
public class GetTimesheetQueryHandler : IRequestHandler<GetTimesheetQuery, Result<TimesheetDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IStringLocalizer<ValidationMessages> _localizer;
    private readonly SwissWorktimePolicy _policy;

    public GetTimesheetQueryHandler(
        IEmployeeRepository employeeRepository,
        ITimeEntryRepository timeEntryRepository,
        IDateTimeProvider dateTimeProvider,
        IStringLocalizer<ValidationMessages> localizer)
    {
        _employeeRepository = employeeRepository;
        _timeEntryRepository = timeEntryRepository;
        _dateTimeProvider = dateTimeProvider;
        _localizer = localizer;
        _policy = new SwissWorktimePolicy();
    }

    public async Task<Result<TimesheetDto>> Handle(GetTimesheetQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result<TimesheetDto>.Failure(Error.NotFound("Employee.NotFound", _localizer["EmployeeNotFound"]));
        }

        var entries = await _timeEntryRepository.GetEntriesForEmployeeRangeAsync(
            request.EmployeeId,
            request.StartDateUtc,
            request.EndDateUtc,
            cancellationToken);

        // Group punches by Swiss local calendar date
        var groupedByDate = entries
            .GroupBy(e => _dateTimeProvider.ToSwissTime(e.ClockInUtc).Date)
            .OrderBy(g => g.Key);

        var days = new List<DaySummaryDto>();
        double totalNet = 0;
        double totalNight = 0;
        double totalSunday = 0;
        var totalRestViolations = 0;
        var totalAmplitudeViolations = 0;

        foreach (var group in groupedByDate)
        {
            var date = group.Key;
            double dayGross = 0;
            var dayBreak = 0;
            double dayNet = 0;
            double dayNight = 0;
            double daySunday = 0;
            var hasRestViolation = false;
            var hasAmplitudeViolation = false;

            var entryDtos = new List<TimeEntryDto>();

            foreach (var entry in group.OrderBy(e => e.ClockInUtc))
            {
                if (entry.ClockOutUtc.HasValue)
                {
                    var gross = (entry.ClockOutUtc.Value - entry.ClockInUtc).TotalHours;
                    var net = gross - (entry.BreakDurationMinutes / 60.0);
                    dayGross += gross;
                    dayBreak += entry.BreakDurationMinutes;
                    dayNet += Math.Max(0, net);

                    dayNight += _policy.CalculateNightHours(entry.ClockInUtc, entry.ClockOutUtc.Value);
                    daySunday += _policy.CalculateSundayHours(entry.ClockInUtc, entry.ClockOutUtc.Value);
                }

                if (entry.DailyRestPeriodViolated)
                {
                    hasRestViolation = true;
                    totalRestViolations++;
                }

                if (entry.DailyAmplitudeExceeded)
                {
                    hasAmplitudeViolation = true;
                    totalAmplitudeViolations++;
                }

                entryDtos.Add(entry.ToDto(_dateTimeProvider));
            }

            // Calculation of daily statutory breakdown
            var dailyContractual = employee.ContractualWeeklyHours / 5.0m;
            var dailyBreakdown = _policy.SplitWorkHours((decimal)dayNet, dailyContractual, employee.StatutoryWeeklyLimit);

            totalNet += dayNet;
            totalNight += dayNight;
            totalSunday += daySunday;

            days.Add(new DaySummaryDto(
                date,
                date.ToString("yyyy-MM-dd"),
                Math.Round(dayGross, 2),
                dayBreak,
                Math.Round(dayNet, 2),
                (double)dailyBreakdown.OrdinaryHours,
                (double)dailyBreakdown.SupplementaryHours,
                (double)dailyBreakdown.StatutoryOvertimeHours,
                Math.Round(dayNight, 2),
                Math.Round(daySunday, 2),
                hasRestViolation,
                hasAmplitudeViolation,
                entryDtos
            ));
        }

        // Global period breakdown
        var periodBreakdown = _policy.SplitWorkHours(
            (decimal)totalNet,
            employee.ContractualWeeklyHours,
            employee.StatutoryWeeklyLimit);

        var dto = new TimesheetDto(
            employee.Id,
            $"{employee.FirstName} {employee.LastName}",
            employee.Oll1Regime,
            employee.ContractualWeeklyHours,
            employee.StatutoryWeeklyLimit,
            request.StartDateUtc,
            request.EndDateUtc,
            Math.Round(totalNet, 2),
            (double)periodBreakdown.OrdinaryHours,
            (double)periodBreakdown.SupplementaryHours,
            (double)periodBreakdown.StatutoryOvertimeHours,
            Math.Round(totalNight, 2),
            Math.Round(totalSunday, 2),
            totalRestViolations,
            totalAmplitudeViolations,
            days
        );

        return Result<TimesheetDto>.Success(dto);
    }
}
