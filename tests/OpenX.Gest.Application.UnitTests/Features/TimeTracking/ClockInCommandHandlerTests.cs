using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.Commands.ClockIn;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class ClockInCommandHandlerTests
{
    private readonly Mock<ITimeEntryRepository> _timeEntryRepoMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly ClockInCommandHandler _handler;

    private readonly Employee _sampleEmployee;

    public ClockInCommandHandlerTests()
    {
        _timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        var localizer = TestLocalizerHelper.CreateMockLocalizer();

        _handler = new ClockInCommandHandler(
            _timeEntryRepoMock.Object,
            _employeeRepoMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object,
            localizer);

        _sampleEmployee = new Employee(
            Guid.NewGuid(),
            "Marco",
            "Rossi",
            "marco.rossi@openx.ch",
            "Operations",
            42.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);
    }

    [Fact]
    public async Task Handle_WhenEmployeeDoesNotExist_ShouldReturnNotFoundFailure()
    {
        // Arrange
        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var command = new ClockInCommand(_sampleEmployee.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenActiveEntryAlreadyExists_ShouldReturnConflictFailure()
    {
        // Arrange
        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(_sampleEmployee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_sampleEmployee);

        var existingActive = new TimeEntry(Guid.NewGuid(), _sampleEmployee.Id, DateTime.UtcNow.AddHours(-2));
        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_sampleEmployee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingActive);

        var command = new ClockInCommand(_sampleEmployee.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeEntry.ActiveExists");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidRequestAndGps_ShouldCreateEntryAndReturnDto()
    {
        // Arrange
        var nowUtc = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(nowUtc);
        _dateTimeProviderMock.Setup(d => d.ToSwissTime(nowUtc)).Returns(nowUtc.AddHours(1));

        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(_sampleEmployee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_sampleEmployee);

        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_sampleEmployee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeEntry?)null);

        _timeEntryRepoMock
            .Setup(r => r.GetPreviousEntryBeforeAsync(_sampleEmployee.Id, nowUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeEntry?)null);

        var command = new ClockInCommand(_sampleEmployee.Id, 46.0037, 8.9511, 10.0, "Office entrance");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EmployeeId.Should().Be(_sampleEmployee.Id);
        result.Value.HasClockInGps.Should().BeTrue();
        result.Value.Notes.Should().Be("Office entrance");

        _timeEntryRepoMock.Verify(r => r.AddAsync(It.IsAny<TimeEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPreviousShiftHasInsufficientRestPeriod_ShouldFlagDailyRestViolation()
    {
        // Arrange (Shift ended 8 hours ago -> Swiss law mandates 11 consecutive hours)
        var nowUtc = new DateTime(2026, 3, 10, 6, 0, 0, DateTimeKind.Utc);
        var prevOutUtc = nowUtc.AddHours(-8);

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(nowUtc);
        _dateTimeProviderMock.Setup(d => d.ToSwissTime(nowUtc)).Returns(nowUtc.AddHours(1));

        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(_sampleEmployee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_sampleEmployee);

        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_sampleEmployee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeEntry?)null);

        var prevEntry = new TimeEntry(Guid.NewGuid(), _sampleEmployee.Id, prevOutUtc.AddHours(-8));
        prevEntry.ClockOut(prevOutUtc, 30);

        _timeEntryRepoMock
            .Setup(r => r.GetPreviousEntryBeforeAsync(_sampleEmployee.Id, nowUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync(prevEntry);

        var command = new ClockInCommand(_sampleEmployee.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DailyRestPeriodViolated.Should().BeTrue();
        result.Value.RestPeriodHoursBeforeShift.Should().Be(8.0);
        result.Value.Violations.HasFlag(ViolationType.DailyRestPeriodViolated).Should().BeTrue();
    }
}
