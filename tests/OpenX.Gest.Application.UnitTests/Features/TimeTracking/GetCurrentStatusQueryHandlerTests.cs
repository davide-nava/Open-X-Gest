using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.Queries.GetCurrentStatus;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class GetCurrentStatusQueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<ITimeEntryRepository> _timeEntryRepoMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly GetCurrentStatusQueryHandler _handler;

    private readonly Employee _employee;

    public GetCurrentStatusQueryHandlerTests()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        var localizer = TestLocalizerHelper.CreateMockLocalizer();

        _handler = new GetCurrentStatusQueryHandler(
            _employeeRepoMock.Object,
            _timeEntryRepoMock.Object,
            _dateTimeProviderMock.Object,
            localizer);

        _employee = new Employee(
            Guid.NewGuid(),
            "Enrico",
            "Fermi",
            "enrico.fermi@openx.ch",
            "Research",
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

        var query = new GetCurrentStatusQuery(missingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotClockedIn_ShouldReturnStatusWithIsClockedInFalse()
    {
        // Arrange
        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(_employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_employee);

        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeEntry?)null);

        var query = new GetCurrentStatusQuery(_employee.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsClockedIn.Should().BeFalse();
        result.Value.ActiveEntryId.Should().BeNull();
        result.Value.ClockInUtc.Should().BeNull();
        result.Value.ElapsedWorkedHoursToday.Should().Be(0);
        result.Value.SuggestedStatutoryBreakMinutes.Should().Be(0);
        result.Value.EmployeeFullName.Should().Be("Enrico Fermi");
    }

    [Fact]
    public async Task Handle_WhenEmployeeIsClockedIn_ShouldCalculateElapsedAndSuggestedBreak()
    {
        // Arrange
        var nowUtc = new DateTime(2026, 3, 10, 14, 0, 0, DateTimeKind.Utc);
        var clockInUtc = nowUtc.AddHours(-7.5); // 7.5 hours elapsed (> 7h) -> requires 30 min statutory break

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(nowUtc);
        _dateTimeProviderMock.Setup(d => d.ToSwissTime(clockInUtc)).Returns(clockInUtc.AddHours(1));

        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(_employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_employee);

        var activeEntry = new TimeEntry(Guid.NewGuid(), _employee.Id, clockInUtc);
        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEntry);

        var query = new GetCurrentStatusQuery(_employee.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsClockedIn.Should().BeTrue();
        result.Value.ActiveEntryId.Should().Be(activeEntry.Id);
        result.Value.ClockInUtc.Should().Be(clockInUtc);
        result.Value.ElapsedWorkedHoursToday.Should().Be(7.5);
        result.Value.SuggestedStatutoryBreakMinutes.Should().Be(30);
    }
}
