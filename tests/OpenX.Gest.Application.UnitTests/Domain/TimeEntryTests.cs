using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;
using OpenX.Gest.Domain.ValueObjects;

namespace OpenX.Gest.Application.UnitTests.Domain;

public class TimeEntryTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var clockIn = DateTime.UtcNow;
        var gps = new GpsCoordinate(46.0037, 8.9511, 10.0, clockIn);

        // Act
        var entry = new TimeEntry(id, employeeId, clockIn, gps, "Test note");

        // Assert
        entry.Id.Should().Be(id);
        entry.EmployeeId.Should().Be(employeeId);
        entry.ClockInUtc.Should().Be(clockIn);
        entry.ClockOutUtc.Should().BeNull();
        entry.BreakDurationMinutes.Should().Be(0);
        entry.PunctualClockInGps.Should().Be(gps);
        entry.Notes.Should().Be("Test note");
        entry.Status.Should().Be(TimeEntryStatus.Open);
        entry.Violations.Should().Be(ViolationType.None);
        entry.AuditTrail.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyEmployeeId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new TimeEntry(Guid.NewGuid(), Guid.Empty, DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("employeeId");
    }

    [Fact]
    public void ClockOut_WithValidData_ShouldSetStatusCompletedAndProperties()
    {
        // Arrange
        var clockIn = DateTime.UtcNow.AddHours(-8);
        var clockOut = DateTime.UtcNow;
        var entry = new TimeEntry(Guid.NewGuid(), Guid.NewGuid(), clockIn);
        var gps = new GpsCoordinate(46.0037, 8.9511, 10.0, clockOut);

        // Act
        var result = entry.ClockOut(clockOut, 30, gps);

        // Assert
        result.IsSuccess.Should().BeTrue();
        entry.ClockOutUtc.Should().Be(clockOut);
        entry.BreakDurationMinutes.Should().Be(30);
        entry.PunctualClockOutGps.Should().Be(gps);
        entry.Status.Should().Be(TimeEntryStatus.Completed);
    }

    [Fact]
    public void ClockOut_WithClockOutPriorToOrEqualClockIn_ShouldReturnFailure()
    {
        // Arrange
        var clockIn = DateTime.UtcNow;
        var entry = new TimeEntry(Guid.NewGuid(), Guid.NewGuid(), clockIn);

        // Act
        var result = entry.ClockOut(clockIn.AddMinutes(-5), 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeEntry.InvalidClockOut");
    }

    [Fact]
    public void ClockOut_WithNegativeBreakDuration_ShouldReturnFailure()
    {
        // Arrange
        var clockIn = DateTime.UtcNow.AddHours(-4);
        var entry = new TimeEntry(Guid.NewGuid(), Guid.NewGuid(), clockIn);

        // Act
        var result = entry.ClockOut(DateTime.UtcNow, -10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeEntry.NegativeBreak");
    }

    [Fact]
    public void ClockOut_WithBreakExceedingShiftDuration_ShouldReturnFailure()
    {
        // Arrange
        var clockIn = DateTime.UtcNow.AddHours(-2); // 120 minutes
        var entry = new TimeEntry(Guid.NewGuid(), Guid.NewGuid(), clockIn);

        // Act
        var result = entry.ClockOut(DateTime.UtcNow, 150);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeEntry.BreakExceedsDuration");
    }

    [Fact]
    public void SetComplianceMetrics_ShouldUpdateFieldsCorrectly()
    {
        // Arrange
        var entry = new TimeEntry(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        // Act
        entry.SetComplianceMetrics(9.5, true, 15.0, true, ViolationType.DailyRestPeriodViolated | ViolationType.DailyAmplitudeExceeded);

        // Assert
        entry.RestPeriodHoursBeforeShift.Should().Be(9.5);
        entry.DailyRestPeriodViolated.Should().BeTrue();
        entry.DailyAmplitudeHours.Should().Be(15.0);
        entry.DailyAmplitudeExceeded.Should().BeTrue();
        entry.Violations.Should().HaveFlag(ViolationType.DailyRestPeriodViolated);
        entry.Violations.Should().HaveFlag(ViolationType.DailyAmplitudeExceeded);
    }

    [Fact]
    public void ApplyCorrection_WithValidData_ShouldAddAuditAndModifyState()
    {
        // Arrange
        var clockIn = DateTime.UtcNow.AddHours(-8);
        var clockOut = DateTime.UtcNow;
        var entry = new TimeEntry(Guid.NewGuid(), Guid.NewGuid(), clockIn);
        entry.ClockOut(clockOut, 30);

        var operatorId = Guid.NewGuid();
        var correctedClockIn = clockIn.AddMinutes(15);
        var correctedClockOut = clockOut.AddMinutes(15);

        // Act
        var result = entry.ApplyCorrection(
            operatorId,
            correctedClockIn,
            correctedClockOut,
            45,
            "Badge dimenticato all'entrata");

        // Assert
        result.IsSuccess.Should().BeTrue();
        entry.ClockInUtc.Should().Be(correctedClockIn);
        entry.ClockOutUtc.Should().Be(correctedClockOut);
        entry.BreakDurationMinutes.Should().Be(45);
        entry.Status.Should().Be(TimeEntryStatus.Approved);
        entry.AuditTrail.Should().HaveCount(1);

        var audit = entry.AuditTrail.First();
        audit.TimeEntryId.Should().Be(entry.Id);
        audit.OperatorId.Should().Be(operatorId);
        audit.PreCorrectionClockInUtc.Should().Be(clockIn);
        audit.PreCorrectionClockOutUtc.Should().Be(clockOut);
        audit.PreCorrectionBreakMinutes.Should().Be(30);
        audit.PostCorrectionClockInUtc.Should().Be(correctedClockIn);
        audit.PostCorrectionClockOutUtc.Should().Be(correctedClockOut);
        audit.PostCorrectionBreakMinutes.Should().Be(45);
        audit.MandatoryReason.Should().Be("Badge dimenticato all'entrata");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ApplyCorrection_WithEmptyReason_ShouldReturnFailure(string? emptyReason)
    {
        // Arrange
        var entry = new TimeEntry(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(-2));

        // Act
        var result = entry.ApplyCorrection(
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-2),
            DateTime.UtcNow,
            0,
            emptyReason);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeCorrection.ReasonRequired");
    }

    [Fact]
    public void ApplyCorrection_WithClockOutPriorToClockIn_ShouldReturnFailure()
    {
        // Arrange
        var entry = new TimeEntry(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(-2));
        var now = DateTime.UtcNow;

        // Act
        var result = entry.ApplyCorrection(
            Guid.NewGuid(),
            now,
            now.AddHours(-1),
            0,
            "Correzione errata");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeCorrection.InvalidDates");
    }
}
