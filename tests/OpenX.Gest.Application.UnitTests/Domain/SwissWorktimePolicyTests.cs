using OpenX.Gest.Domain.Enums;
using OpenX.Gest.Domain.Services;

namespace OpenX.Gest.Application.UnitTests.Domain;

public class SwissWorktimePolicyTests
{
    private readonly SwissWorktimePolicy _policy = new();

    [Theory]
    [InlineData(4.0, 0)]
    [InlineData(5.5, 0)]
    [InlineData(5.6, 15)]
    [InlineData(7.0, 15)]
    [InlineData(7.1, 30)]
    [InlineData(9.0, 30)]
    [InlineData(9.5, 60)]
    [InlineData(12.0, 60)]
    public void CalculateStatutoryBreakMinutes_ShouldReturnCorrectBreakTier(double workedHours, int expectedBreakMinutes)
    {
        // Act
        var result = _policy.CalculateStatutoryBreakMinutes(workedHours);

        // Assert
        result.Should().Be(expectedBreakMinutes);
    }

    [Fact]
    public void EvaluateDailyAmplitude_WhenUnder14Hours_ShouldNotBeExceeded()
    {
        // Arrange
        var start = new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(10);

        // Act
        var (amplitude, exceeded) = _policy.EvaluateDailyAmplitude(start, end);

        // Assert
        amplitude.Should().Be(10.0);
        exceeded.Should().BeFalse();
    }

    [Fact]
    public void EvaluateDailyAmplitude_WhenOver14Hours_ShouldBeExceeded()
    {
        // Arrange
        var start = new DateTime(2026, 5, 1, 6, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(14.5);

        // Act
        var (amplitude, exceeded) = _policy.EvaluateDailyAmplitude(start, end);

        // Assert
        amplitude.Should().Be(14.5);
        exceeded.Should().BeTrue();
    }

    [Fact]
    public void EvaluateDailyRestPeriod_WhenOver11Hours_ShouldNotBeViolated()
    {
        // Arrange
        var prevEnd = new DateTime(2026, 5, 1, 18, 0, 0, DateTimeKind.Utc);
        var currentStart = new DateTime(2026, 5, 2, 7, 0, 0, DateTimeKind.Utc); // 13h rest

        // Act
        var (restHours, violated) = _policy.EvaluateDailyRestPeriod(prevEnd, currentStart);

        // Assert
        restHours.Should().Be(13.0);
        violated.Should().BeFalse();
    }

    [Fact]
    public void EvaluateDailyRestPeriod_WhenUnder11Hours_ShouldBeViolated()
    {
        // Arrange
        var prevEnd = new DateTime(2026, 5, 1, 22, 0, 0, DateTimeKind.Utc);
        var currentStart = new DateTime(2026, 5, 2, 6, 30, 0, DateTimeKind.Utc); // 8.5h rest

        // Act
        var (restHours, violated) = _policy.EvaluateDailyRestPeriod(prevEnd, currentStart);

        // Assert
        restHours.Should().Be(8.5);
        violated.Should().BeTrue();
    }

    [Fact]
    public void CalculateNightHours_WithDaytimeShift_ShouldReturnZero()
    {
        // Arrange (10:00 to 18:00 UTC = 12:00 to 20:00 CEST)
        var start = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(8);

        // Act
        var nightHours = _policy.CalculateNightHours(start, end);

        // Assert
        nightHours.Should().Be(0);
    }

    [Fact]
    public void CalculateNightHours_WithOvernightShift_ShouldComputeNightHoursAccurately()
    {
        // In Swiss summer time (CEST = UTC+2):
        // 21:00 UTC = 23:00 CEST (Night start)
        // 03:00 UTC = 05:00 CEST (Night)
        // Night period in Switzerland: 23:00 to 06:00
        var start = new DateTime(2026, 6, 10, 21, 0, 0, DateTimeKind.Utc); // 23:00 CEST
        var end = new DateTime(2026, 6, 11, 3, 0, 0, DateTimeKind.Utc);   // 05:00 CEST (6 hours night)

        // Act
        var nightHours = _policy.CalculateNightHours(start, end);

        // Assert
        nightHours.Should().Be(6.0);
    }

    [Fact]
    public void CalculateNightHours_WhenEndBeforeStart_ShouldReturnZero()
    {
        var start = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        var night = _policy.CalculateNightHours(start, start.AddHours(-1));
        night.Should().Be(0);
    }

    [Fact]
    public void CalculateSundayHours_WithStandardWeekday_ShouldReturnZero()
    {
        // Wednesday shift
        var start = new DateTime(2026, 5, 6, 8, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(8);

        // Act
        var sundayHours = _policy.CalculateSundayHours(start, end);

        // Assert
        sundayHours.Should().Be(0);
    }

    [Fact]
    public void CalculateSundayHours_WhenEndBeforeStart_ShouldReturnZero()
    {
        var start = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Utc);
        var sunday = _policy.CalculateSundayHours(start, start.AddHours(-1));
        sunday.Should().Be(0);
    }

    [Fact]
    public void SplitWorkHours_WhenTotalLessThanContractual_ShouldBeAllOrdinary()
    {
        // Act
        var breakdown = _policy.SplitWorkHours(38.0m, 40.0m, StatutoryWeeklyLimit.Hours45);

        // Assert
        breakdown.TotalWorkedHours.Should().Be(38.0m);
        breakdown.OrdinaryHours.Should().Be(38.0m);
        breakdown.SupplementaryHours.Should().Be(0m);
        breakdown.StatutoryOvertimeHours.Should().Be(0m);
    }

    [Fact]
    public void SplitWorkHours_WhenExceedsContractual_ShouldSplitBetweenSupplementaryAndOvertime()
    {
        // Act (Contractual 40h, Legal Limit 45h, Worked 48h -> 40 ordinary, 5 Überstunden, 3 Überzeit)
        var breakdown = _policy.SplitWorkHours(48.0m, 40.0m, StatutoryWeeklyLimit.Hours45);

        // Assert
        breakdown.TotalWorkedHours.Should().Be(48.0m);
        breakdown.OrdinaryHours.Should().Be(40.0m);
        breakdown.SupplementaryHours.Should().Be(5.0m);
        breakdown.StatutoryOvertimeHours.Should().Be(3.0m);
    }

    [Fact]
    public void SplitWorkHours_WhenContractualEqualsLegalLimit_ShouldProduceDirectOvertime()
    {
        // Act (Contractual 45h, Legal Limit 45h, Worked 49h -> 45 ordinary, 0 Überstunden, 4 Überzeit)
        var breakdown = _policy.SplitWorkHours(49.0m, 45.0m, StatutoryWeeklyLimit.Hours45);

        // Assert
        breakdown.TotalWorkedHours.Should().Be(49.0m);
        breakdown.OrdinaryHours.Should().Be(45.0m);
        breakdown.SupplementaryHours.Should().Be(0m);
        breakdown.StatutoryOvertimeHours.Should().Be(4.0m);
    }
}
