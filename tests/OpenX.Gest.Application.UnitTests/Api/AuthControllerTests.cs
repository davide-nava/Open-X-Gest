using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OpenX.Gest.Api.Controllers;
using OpenX.Gest.Application.Features.Auth.Commands;
using OpenX.Gest.Application.Features.Auth.DTOs;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Api;

public class AuthControllerTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _senderMock = new Mock<ISender>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ISender))).Returns(_senderMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        _controller = new AuthController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnOkWithToken()
    {
        // Arrange
        var command = new LoginCommand("employee@openx.ch", "Password123!");
        var responseDto = new AuthResponseDto(
            "fake-jwt-token",
            Guid.NewGuid(),
            "Employee Name",
            "employee@openx.ch",
            "Employee",
            LanguageCode.It,
            Oll1Regime.StandardRecord);

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponseDto>.Success(responseDto));

        // Act
        var result = await _controller.Login(command, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(responseDto);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldReturnUnauthorizedProblemDetails()
    {
        // Arrange
        var command = new LoginCommand("wrong@openx.ch", "WrongPassword");
        var failure = Result<AuthResponseDto>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Credenziali non valide."));

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failure);

        // Act
        var result = await _controller.Login(command, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Title.Should().Be("Auth.InvalidCredentials");
    }
}
