using System.Text;
using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.Queries.ExportSecoReport;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class ExportSecoReportQueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<ITimeEntryRepository> _timeEntryRepoMock;
    private readonly Mock<ISecoComplianceExportService> _exportServiceMock;
    private readonly ExportSecoReportQueryHandler _handler;

    private readonly Employee _employee;

    public ExportSecoReportQueryHandlerTests()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        _exportServiceMock = new Mock<ISecoComplianceExportService>();
        var localizer = TestLocalizerHelper.CreateMockLocalizer();

        _handler = new ExportSecoReportQueryHandler(
            _employeeRepoMock.Object,
            _timeEntryRepoMock.Object,
            _exportServiceMock.Object,
            localizer);

        _employee = new Employee(
            Guid.NewGuid(),
            "Jean",
            "Dupont",
            "jean.dupont@openx.ch",
            "Compliance",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.Fr);
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var query = new ExportSecoReportQuery(missingId, 2026, 3, "csv", "fr");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
    }

    [Fact]
    public async Task Handle_WithCsvFormat_ShouldGenerateCsvReportAndReturnDto()
    {
        // Arrange
        var fakeBytes = Encoding.UTF8.GetBytes("Date;Start;End;Net");
        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(_employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_employee);

        _timeEntryRepoMock
            .Setup(r => r.GetEntriesForEmployeeRangeAsync(_employee.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimeEntry>());

        _exportServiceMock
            .Setup(s => s.GenerateCsvReportAsync(_employee, It.IsAny<List<TimeEntry>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), "fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeBytes);

        var query = new ExportSecoReportQuery(_employee.Id, 2026, 3, "csv", "fr");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("text/csv");
        result.Value.FileName.Should().Contain("SECO_Report_Dupont_2026_03.csv");
        result.Value.FileBytes.Should().Equal(fakeBytes);
    }

    [Fact]
    public async Task Handle_WithPdfFormat_ShouldGeneratePdfReportAndReturnDto()
    {
        // Arrange
        var fakeBytes = Encoding.UTF8.GetBytes("%PDF-1.4 mock content");
        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(_employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_employee);

        _timeEntryRepoMock
            .Setup(r => r.GetEntriesForEmployeeRangeAsync(_employee.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimeEntry>());

        _exportServiceMock
            .Setup(s => s.GenerateInspectionSummaryPdfAsync(_employee, It.IsAny<List<TimeEntry>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), "it", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeBytes);

        var query = new ExportSecoReportQuery(_employee.Id, 2026, 3, "pdf", "it");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/pdf");
        result.Value.FileName.Should().Contain("SECO_Report_Dupont_2026_03.pdf");
        result.Value.FileBytes.Should().Equal(fakeBytes);
    }
}
