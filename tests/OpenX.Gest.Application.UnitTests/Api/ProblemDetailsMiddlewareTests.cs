using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OpenX.Gest.Api.Middleware;

namespace OpenX.Gest.Application.UnitTests.Api;

public class ProblemDetailsMiddlewareTests
{
    private readonly Mock<ILogger<ProblemDetailsMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldExecuteNextSuccessfully()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextExecuted = false;
        RequestDelegate next = _ =>
        {
            nextExecuted = true;
            return Task.CompletedTask;
        };

        var middleware = new ProblemDetailsMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionOccurs_ShouldReturn500ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = _ => throw new InvalidOperationException("Simulated unexpected failure");
        var middleware = new ProblemDetailsMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        responseBody.Should().Contain("Errore interno del server");
        responseBody.Should().Contain("Simulated unexpected failure");
    }
}
