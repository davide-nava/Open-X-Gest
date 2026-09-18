using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.Commands.ClockOut;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class ClockOutCommandHandlerTests
{
    private readonly Mock<ITimeEntryRepository> _timeEntryRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly ClockOutCommandHandler _handler;

    private readonly Guid _employeeId = Guid.NewGuid();

    public ClockOutCommandHandlerTests()
    {
        _timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        var localizer = TestLocalizerHelper.CreateMockLocalizer();

        _handler = new ClockOutCommandHandler(
            _timeEntryRepoMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object,
            localizer);
    }

    [Fact]
    public async Task Handle_WhenNoActiveShiftFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeEntry?)null);

        var command = new ClockOutCommand(_employeeId, 30);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeEntry.NotFound");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenClockOutBeforeClockIn_ShouldReturnFailure()
    {
        // Arrange
        var clockInUtc = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);
        var activeEntry = new TimeEntry(Guid.NewGuid(), _employeeId, clockInUtc);

        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEntry);

        // Mock current time earlier than clockIn
        var earlierTimeUtc = clockInUtc.AddMinutes(-30);
        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(earlierTimeUtc);

        var command = new ClockOutCommand(_employeeId, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeEntry.InvalidClockOut");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNormalShiftAndBreak_ShouldCompleteShiftSuccessfully()
    {
        // Arrange
        var clockInUtc = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var clockOutUtc = clockInUtc.AddHours(8); // 8h shift requires 30m break
        var activeEntry = new TimeEntry(Guid.NewGuid(), _employeeId, clockInUtc);

        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEntry);

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(clockOutUtc);
        _dateTimeProviderMock.Setup(d => d.ToSwissTime(It.IsAny<DateTime>())).Returns<DateTime>(dt => dt.AddHours(1));

        var command = new ClockOutCommand(_employeeId, 30, 46.0037, 8.9511, 10.0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ClockOutUtc.Should().Be(clockOutUtc);
        result.Value.BreakDurationMinutes.Should().Be(30);
        result.Value.HasClockOutGps.Should().BeTrue();
        result.Value.DailyAmplitudeExceeded.Should().BeFalse();
        result.Value.Violations.HasFlag(ViolationType.DailyAmplitudeExceeded).Should().BeFalse();
        result.Value.Violations.HasFlag(ViolationType.InsufficientBreak).Should().BeFalse();

        _timeEntryRepoMock.Verify(r => r.Update(activeEntry), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAmplitudeExceeds14Hours_ShouldFlagDailyAmplitudeExceeded()
    {
        // Arrange (15 hour shift: amplitude exceeds 14h statutory max)
        var clockInUtc = new DateTime(2026, 3, 10, 6, 0, 0, DateTimeKind.Utc);
        var clockOutUtc = clockInUtc.AddHours(15);
        var activeEntry = new TimeEntry(Guid.NewGuid(), _employeeId, clockInUtc);

        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEntry);

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(clockOutUtc);
        _dateTimeProviderMock.Setup(d => d.ToSwissTime(It.IsAny<DateTime>())).Returns<DateTime>(dt => dt.AddHours(1));

        var command = new ClockOutCommand(_employeeId, 60);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DailyAmplitudeExceeded.Should().BeTrue();
        result.Value.Violations.HasFlag(ViolationType.DailyAmplitudeExceeded).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenBreakIsInsufficient_ShouldFlagInsufficientBreakViolation()
    {
        // Arrange (8 hours worked requires 30m break, but employee recorded 10m)
        var clockInUtc = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var clockOutUtc = clockInUtc.AddHours(8);
        var activeEntry = new TimeEntry(Guid.NewGuid(), _employeeId, clockInUtc);

        _timeEntryRepoMock
            .Setup(r => r.GetActiveEntryForEmployeeAsync(_employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEntry);

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(clockOutUtc);
        _dateTimeProviderMock.Setup(d => d.ToSwissTime(It.IsAny<DateTime>())).Returns<DateTime>(dt => dt.AddHours(1));

        var command = new ClockOutCommand(_employeeId, 10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Violations.HasFlag(ViolationType.InsufficientBreak).Should().BeTrue();
    }
}
