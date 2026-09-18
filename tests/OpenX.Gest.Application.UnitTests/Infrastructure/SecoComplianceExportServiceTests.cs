using System.Text;
using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;
using OpenX.Gest.Infrastructure.Services;

namespace OpenX.Gest.Application.UnitTests.Infrastructure;

public class SecoComplianceExportServiceTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly SecoComplianceExportService _service;

    private readonly Employee _employee;

    public SecoComplianceExportServiceTests()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _service = new SecoComplianceExportService(_dateTimeProviderMock.Object);

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(new DateTime(2026, 3, 10, 18, 0, 0, DateTimeKind.Utc));
        _dateTimeProviderMock.Setup(d => d.ToSwissTime(It.IsAny<DateTime>())).Returns<DateTime>(dt => dt.AddHours(1));

        _employee = new Employee(
            Guid.NewGuid(),
            "Beat",
            "Zimmermann",
            "beat.zimmermann@openx.ch",
            "Logistics",
            42.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.De);
    }

    [Theory]
    [InlineData("de", "Datum;Beginn;Ende")]
    [InlineData("fr", "Date;Debut;Fin")]
    [InlineData("en", "Date;Start_Time;End_Time")]
    [InlineData("it", "Data;Ora_Inizio;Ora_Fine")]
    public async Task GenerateCsvReportAsync_WithDifferentLanguages_ShouldProduceLocalizedHeader(string lang, string expectedHeaderPrefix)
    {
        // Arrange
        var startUtc = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);

        var inUtc = new DateTime(2026, 3, 2, 8, 0, 0, DateTimeKind.Utc);
        var outUtc = inUtc.AddHours(8.5);
        var entry = new TimeEntry(Guid.NewGuid(), _employee.Id, inUtc);
        entry.ClockOut(outUtc, 30);

        var entries = new List<TimeEntry> { entry };

        // Act
        var bytes = await _service.GenerateCsvReportAsync(_employee, entries, startUtc, endUtc, lang);
        var csv = Encoding.UTF8.GetString(bytes);

        // Assert
        csv.Should().Contain(expectedHeaderPrefix);
        csv.Should().Contain("Zimmermann Beat");
        csv.Should().Contain("Logistics");
        csv.Should().Contain("8.00"); // Net worked hours
    }

    [Fact]
    public async Task GenerateCsvReportAsync_WhenViolationsOccur_ShouldIncludeViolationsText()
    {
        // Arrange
        var startUtc = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);

        var inUtc = new DateTime(2026, 3, 2, 6, 0, 0, DateTimeKind.Utc);
        var outUtc = inUtc.AddHours(15);
        var entry = new TimeEntry(Guid.NewGuid(), _employee.Id, inUtc);
        entry.ClockOut(outUtc, 10);
        entry.SetComplianceMetrics(8.0, true, 15.0, true, ViolationType.DailyRestPeriodViolated | ViolationType.DailyAmplitudeExceeded | ViolationType.InsufficientBreak);

        var entries = new List<TimeEntry> { entry };

        // Act
        var bytes = await _service.GenerateCsvReportAsync(_employee, entries, startUtc, endUtc, "it");
        var csv = Encoding.UTF8.GetString(bytes);

        // Assert
        csv.Should().Contain("Rest <11h");
        csv.Should().Contain("Amplitude >14h");
        csv.Should().Contain("Break insufficient");
    }

    [Fact]
    public async Task GenerateInspectionSummaryPdfAsync_ShouldIncludeLegalHeadersAndSignatures()
    {
        // Arrange
        var startUtc = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);

        var inUtc = new DateTime(2026, 3, 2, 8, 0, 0, DateTimeKind.Utc);
        var outUtc = inUtc.AddHours(8.5);
        var entry = new TimeEntry(Guid.NewGuid(), _employee.Id, inUtc);
        entry.ClockOut(outUtc, 30);

        var entries = new List<TimeEntry> { entry };

        // Act
        var bytes = await _service.GenerateInspectionSummaryPdfAsync(_employee, entries, startUtc, endUtc, "it");
        var text = Encoding.UTF8.GetString(bytes);

        // Assert
        text.Should().Contain("SCHEDA DI REGISTRAZIONE DELL'ORARIO DI LAVORO (SECO - Art. 73 OLL 1)");
        text.Should().Contain("Zimmermann Beat");
        text.Should().Contain("CONFORME");
        text.Should().Contain("Firma del Datore di Lavoro");
        text.Should().Contain("Firma del Collaboratore");
    }
}
