using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OpenX.Gest.Api.Controllers;
using OpenX.Gest.Application.Features.TimeTracking.Commands.ClockIn;
using OpenX.Gest.Application.Features.TimeTracking.Commands.ClockOut;
using OpenX.Gest.Application.Features.TimeTracking.Commands.CorrectTimeEntry;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Application.Features.TimeTracking.Queries.ExportSecoReport;
using OpenX.Gest.Application.Features.TimeTracking.Queries.GetCurrentStatus;
using OpenX.Gest.Application.Features.TimeTracking.Queries.GetTimesheet;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Api;

public class TimeTrackingControllerTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly TimeTrackingController _controller;

    public TimeTrackingControllerTests()
    {
        _senderMock = new Mock<ISender>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ISender))).Returns(_senderMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        _controller = new TimeTrackingController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Fact]
    public async Task ClockIn_WhenSuccessful_ShouldReturnOkWithTimeEntryDto()
    {
        // Arrange
        var command = new ClockInCommand(Guid.NewGuid());
        var dto = new TimeEntryDto(
            Guid.NewGuid(),
            command.EmployeeId,
            DateTime.UtcNow,
            null,
            DateTime.UtcNow.AddHours(1),
            null,
            0,
            0.0,
            false,
            false,
            null,
            false,
            null,
            false,
            null,
            TimeEntryStatus.Open,
            ViolationType.None,
            new List<TimeCorrectionAuditDto>());

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TimeEntryDto>.Success(dto));

        // Act
        var result = await _controller.ClockIn(command, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task ClockOut_WhenSuccessful_ShouldReturnOkWithTimeEntryDto()
    {
        // Arrange
        var command = new ClockOutCommand(Guid.NewGuid(), 30);
        var dto = new TimeEntryDto(
            Guid.NewGuid(),
            command.EmployeeId,
            DateTime.UtcNow.AddHours(-8),
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(-7),
            DateTime.UtcNow.AddHours(1),
            30,
            7.5,
            false,
            false,
            null,
            false,
            8.0,
            false,
            null,
            TimeEntryStatus.Completed,
            ViolationType.None,
            new List<TimeCorrectionAuditDto>());

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TimeEntryDto>.Success(dto));

        // Act
        var result = await _controller.ClockOut(command, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Correct_WhenSuccessful_ShouldReturnOkWithUpdatedTimeEntryDto()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var timeEntryId = Guid.NewGuid();
        var command = new CorrectTimeEntryCommand(timeEntryId, operatorId, DateTime.UtcNow.AddHours(-8), DateTime.UtcNow, 30, "Rettifica timbratura");
        var dto = new TimeEntryDto(
            command.TimeEntryId,
            Guid.NewGuid(),
            command.NewClockInUtc,
            command.NewClockOutUtc,
            command.NewClockInUtc.AddHours(1),
            command.NewClockOutUtc?.AddHours(1),
            30,
            7.5,
            false,
            false,
            null,
            false,
            8.0,
            false,
            null,
            TimeEntryStatus.Approved,
            ViolationType.None,
            new List<TimeCorrectionAuditDto>());

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TimeEntryDto>.Success(dto));

        // Act
        var result = await _controller.Correct(command, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetStatus_WhenFound_ShouldReturnCurrentStatus()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var statusDto = new CurrentTimeStatusDto(
            employeeId,
            true,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-2),
            DateTime.UtcNow.AddHours(-1),
            2.0,
            0,
            Oll1Regime.StandardRecord,
            "Mario Rossi");

        _senderMock
            .Setup(s => s.Send(It.Is<GetCurrentStatusQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CurrentTimeStatusDto>.Success(statusDto));

        // Act
        var result = await _controller.GetStatus(employeeId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(statusDto);
    }

    [Fact]
    public async Task GetTimesheet_WhenFound_ShouldReturnTimesheetDto()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var startUtc = DateTime.UtcNow.AddDays(-7);
        var endUtc = DateTime.UtcNow;

        var timesheetDto = new TimesheetDto(
            employeeId,
            "Mario Rossi",
            Oll1Regime.StandardRecord,
            42.0m,
            StatutoryWeeklyLimit.Hours45,
            startUtc,
            endUtc,
            40.0,
            40.0,
            0,
            0,
            0,
            0,
            0,
            0,
            new List<DaySummaryDto>());

        _senderMock
            .Setup(s => s.Send(It.Is<GetTimesheetQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TimesheetDto>.Success(timesheetDto));

        // Act
        var result = await _controller.GetTimesheet(employeeId, startUtc, endUtc, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(timesheetDto);
    }

    [Fact]
    public async Task ExportSeco_WhenSuccess_ShouldReturnFileContentResult()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var exportDto = new SecoExportDto("report.csv", "text/csv", new byte[] { 1, 2, 3 });

        _senderMock
            .Setup(s => s.Send(It.Is<ExportSecoReportQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SecoExportDto>.Success(exportDto));

        // Act
        var result = await _controller.ExportSeco(employeeId, 2026, 3, "csv", "it", CancellationToken.None);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/csv");
        fileResult.FileDownloadName.Should().Be("report.csv");
        fileResult.FileContents.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task ExportSeco_WhenFailure_ShouldReturnProblemDetails()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<ExportSecoReportQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SecoExportDto>.Failure(Error.NotFound("Employee.NotFound", "Dipendente non trovato")));

        // Act
        var result = await _controller.ExportSeco(employeeId, 2026, 3, "csv", "it", CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
