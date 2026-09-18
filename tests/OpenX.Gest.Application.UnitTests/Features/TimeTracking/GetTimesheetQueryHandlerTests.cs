using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.Queries.GetTimesheet;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class GetTimesheetQueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<ITimeEntryRepository> _timeEntryRepoMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly GetTimesheetQueryHandler _handler;

    private readonly Employee _employee;

    public GetTimesheetQueryHandlerTests()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        var localizer = TestLocalizerHelper.CreateMockLocalizer();

        _handler = new GetTimesheetQueryHandler(
            _employeeRepoMock.Object,
            _timeEntryRepoMock.Object,
            _dateTimeProviderMock.Object,
            localizer);

        _employee = new Employee(
            Guid.NewGuid(),
            "Matteo",
            "Fontana",
            "matteo.fontana@openx.ch",
            "Architecture",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var query = new GetTimesheetQuery(missingId, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
    }

    [Fact]
    public async Task Handle_WithNoEntries_ShouldReturnEmptyTimesheet()
    {
        // Arrange
        var startUtc = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2026, 3, 7, 23, 59, 59, DateTimeKind.Utc);

        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(_employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_employee);

        _timeEntryRepoMock
            .Setup(r => r.GetEntriesForEmployeeRangeAsync(_employee.Id, startUtc, endUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimeEntry>());

        var query = new GetTimesheetQuery(_employee.Id, startUtc, endUtc);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EmployeeId.Should().Be(_employee.Id);
        result.Value.TotalNetWorkedHours.Should().Be(0);
        result.Value.Days.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithEntriesAndViolations_ShouldCalculateSummariesAndBreakdowns()
    {
        // Arrange
        var startUtc = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc); // Monday
        var endUtc = new DateTime(2026, 3, 8, 23, 59, 59, DateTimeKind.Utc);   // Sunday

        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(_employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_employee);

        _dateTimeProviderMock
            .Setup(d => d.ToSwissTime(It.IsAny<DateTime>()))
            .Returns<DateTime>(dt => dt.AddHours(1)); // UTC+1

        // Day 1: 8h work, 30m break, normal
        var d1In = new DateTime(2026, 3, 2, 8, 0, 0, DateTimeKind.Utc);
        var d1Out = d1In.AddHours(8.5);
        var entry1 = new TimeEntry(Guid.NewGuid(), _employee.Id, d1In);
        entry1.ClockOut(d1Out, 30);

        // Day 2: 12h work with rest period violation
        var d2In = new DateTime(2026, 3, 3, 7, 0, 0, DateTimeKind.Utc);
        var d2Out = d2In.AddHours(12);
        var entry2 = new TimeEntry(Guid.NewGuid(), _employee.Id, d2In);
        entry2.ClockOut(d2Out, 60);
        entry2.SetComplianceMetrics(8.0, true, 12.0, false, ViolationType.DailyRestPeriodViolated);

        var entries = new List<TimeEntry> { entry1, entry2 };

        _timeEntryRepoMock
            .Setup(r => r.GetEntriesForEmployeeRangeAsync(_employee.Id, startUtc, endUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var query = new GetTimesheetQuery(_employee.Id, startUtc, endUtc);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Days.Should().HaveCount(2);
        result.Value.TotalNetWorkedHours.Should().Be(19.0); // 8.0h + 11.0h
        result.Value.TotalRestViolations.Should().Be(1);
        result.Value.TotalOrdinaryHours.Should().Be(19.0);
    }
}
