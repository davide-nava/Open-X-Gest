using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Services;

namespace OpenX.Gest.Application.Features.TimeTracking.Queries.GetCurrentStatus;

/// <summary>
/// MediatR request handler for retrieving the live attendance status of an employee.
/// </summary>
public class GetCurrentStatusQueryHandler : IRequestHandler<GetCurrentStatusQuery, Result<CurrentTimeStatusDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IStringLocalizer<ValidationMessages> _localizer;
    private readonly SwissWorktimePolicy _policy;

    public GetCurrentStatusQueryHandler(
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

    public async Task<Result<CurrentTimeStatusDto>> Handle(GetCurrentStatusQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result<CurrentTimeStatusDto>.Failure(Error.NotFound("Employee.NotFound", _localizer["EmployeeNotFound"]));
        }

        var activeEntry = await _timeEntryRepository.GetActiveEntryForEmployeeAsync(request.EmployeeId, cancellationToken);

        var isClockedIn = activeEntry != null;
        DateTime? clockInUtc = activeEntry?.ClockInUtc;
        DateTime? clockInSwiss = clockInUtc.HasValue ? _dateTimeProvider.ToSwissTime(clockInUtc.Value) : null;

        double elapsedWorkedHours = 0;
        int suggestedBreakMinutes = 0;

        if (isClockedIn && clockInUtc.HasValue)
        {
            elapsedWorkedHours = Math.Round((_dateTimeProvider.UtcNow - clockInUtc.Value).TotalHours, 2);
            suggestedBreakMinutes = _policy.CalculateStatutoryBreakMinutes(elapsedWorkedHours);
        }

        var dto = new CurrentTimeStatusDto(
            employee.Id,
            isClockedIn,
            activeEntry?.Id,
            clockInUtc,
            clockInSwiss,
            elapsedWorkedHours,
            suggestedBreakMinutes,
            employee.Oll1Regime,
            $"{employee.FirstName} {employee.LastName}"
        );

        return Result<CurrentTimeStatusDto>.Success(dto);
    }
}
