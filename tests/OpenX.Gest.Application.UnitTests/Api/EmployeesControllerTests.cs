using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OpenX.Gest.Api.Controllers;
using OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeLanguage;
using OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeRegime;
using OpenX.Gest.Application.Features.Employees.DTOs;
using OpenX.Gest.Application.Features.Employees.Queries.GetEmployeeById;
using OpenX.Gest.Application.Features.Employees.Queries.GetEmployees;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Api;

public class EmployeesControllerTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly EmployeesController _controller;

    public EmployeesControllerTests()
    {
        _senderMock = new Mock<ISender>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ISender))).Returns(_senderMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        _controller = new EmployeesController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Fact]
    public async Task GetEmployees_ShouldReturnOkWithEmployeesList()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Carlo",
            "Rossi",
            "carlo@openx.ch",
            "Engineering",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        var dtos = new List<EmployeeDto> { EmployeeDto.FromEntity(employee) };

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetEmployeesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<EmployeeDto>>.Success(dtos));

        // Act
        var result = await _controller.GetEmployees(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetEmployeeById_WhenNotFound_ShouldReturnNotFoundProblemDetails()
    {
        // Arrange
        var id = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<GetEmployeeByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmployeeDto>.Failure(Error.NotFound("Employee.NotFound", "Dipendente non trovato")));

        // Act
        var result = await _controller.GetEmployeeById(id, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateRegime_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<UpdateEmployeeRegimeCommand>(c => c.EmployeeId == id && c.Regime == Oll1Regime.SimplifiedRecord), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.UpdateRegime(id, Oll1Regime.SimplifiedRecord, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task UpdateLanguage_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<UpdateEmployeeLanguageCommand>(c => c.EmployeeId == id && c.Language == LanguageCode.De), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.UpdateLanguage(id, LanguageCode.De, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
