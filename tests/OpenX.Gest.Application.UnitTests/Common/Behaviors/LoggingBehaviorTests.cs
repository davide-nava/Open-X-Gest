using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using OpenX.Gest.Application.Common.Behaviors;

namespace OpenX.Gest.Application.UnitTests.Common.Behaviors;

public class LoggingBehaviorTests
{
    public record SampleLoggedCommand(string Value) : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldLogExecutionAndCompletionAndReturnResponse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<SampleLoggedCommand, string>>>();
        var behavior = new LoggingBehavior<SampleLoggedCommand, string>(loggerMock.Object);
        var command = new SampleLoggedCommand("TestValue");

        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("ResponseValue");
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("ResponseValue");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() != null && v.ToString().Contains("OpenX.Gest CQRS Executing")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() != null && v.ToString().Contains("OpenX.Gest CQRS Completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
