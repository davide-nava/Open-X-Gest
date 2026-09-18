using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.Commands.CorrectTimeEntry;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class CorrectTimeEntryCommandHandlerTests
{
    private readonly Mock<ITimeEntryRepository> _timeEntryRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly CorrectTimeEntryCommandHandler _handler;

    private readonly Guid _operatorId = Guid.NewGuid();
    private readonly Guid _employeeId = Guid.NewGuid();

    public CorrectTimeEntryCommandHandlerTests()
    {
        _timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(c => c.UserId).Returns(_operatorId);

        var localizer = TestLocalizerHelper.CreateMockLocalizer();

        _handler = new CorrectTimeEntryCommandHandler(
            _timeEntryRepoMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object,
            _currentUserMock.Object,
            localizer);
    }

    [Fact]
    public async Task Handle_WhenTimeEntryNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _timeEntryRepoMock
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeEntry?)null);

        var command = new CorrectTimeEntryCommand(
            missingId,
            _operatorId,
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow,
            30,
            "Rettifica orario timbratura non registrata");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeEntry.NotFound");
    }

    [Fact]
    public async Task Handle_WhenDomainCorrectionFails_ShouldReturnDomainError()
    {
        // Arrange (Clock-out prior to clock-in)
        var clockIn = new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc);
        var entry = new TimeEntry(Guid.NewGuid(), _employeeId, clockIn);

        _timeEntryRepoMock
            .Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        var command = new CorrectTimeEntryCommand(
            entry.Id,
            _operatorId,
            clockIn,
            clockIn.AddHours(-1), // Invalid clock out
            0,
            "Rettifica errata");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeCorrection.InvalidDates");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidCorrection_ShouldApplyChangesAddAuditAndRecalculateCompliance()
    {
        // Arrange
        var clockIn = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var clockOut = clockIn.AddHours(8);
        var entry = new TimeEntry(Guid.NewGuid(), _employeeId, clockIn);
        entry.ClockOut(clockOut, 30);

        _timeEntryRepoMock
            .Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        var correctedIn = clockIn.AddHours(-1); // 7:00
        var correctedOut = clockIn.AddHours(9);  // 17:00 (10h elapsed -> requires 60m break, but set to 15m)

        _dateTimeProviderMock.Setup(d => d.ToSwissTime(It.IsAny<DateTime>())).Returns<DateTime>(dt => dt.AddHours(1));

        var command = new CorrectTimeEntryCommand(
            entry.Id,
            Guid.Empty, // Test fallback to _currentUserMock
            correctedIn,
            correctedOut,
            15,
            "Rettifica autorizzata da HR");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ClockInUtc.Should().Be(correctedIn);
        result.Value.ClockOutUtc.Should().Be(correctedOut);
        result.Value.BreakDurationMinutes.Should().Be(15);
        result.Value.Violations.HasFlag(ViolationType.InsufficientBreak).Should().BeTrue();

        _timeEntryRepoMock.Verify(r => r.AddCorrectionAuditAsync(It.IsAny<TimeCorrectionAudit>(), It.IsAny<CancellationToken>()), Times.Once);
        _timeEntryRepoMock.Verify(r => r.Update(entry), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
