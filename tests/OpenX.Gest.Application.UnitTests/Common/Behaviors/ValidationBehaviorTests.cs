using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using OpenX.Gest.Application.Common.Behaviors;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    public record SampleCommand(string Data) : IRequest<Result>;
    public record SampleGenericCommand(string Data) : IRequest<Result<string>>;
    public record SamplePrimitiveCommand(string Data) : IRequest<string>;

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<SampleCommand, Result>(Enumerable.Empty<IValidator<SampleCommand>>());
        var command = new SampleCommand("Test");
        var nextCalled = false;
        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationSucceeds_ShouldCallNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<SampleCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<SampleCommand, Result>(new[] { validatorMock.Object });
        var command = new SampleCommand("Test");
        var nextCalled = false;
        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationFailsAndResponseIsResult_ShouldReturnResultFailureWithoutThrowing()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<SampleCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Data", "Data is required") }));

        var behavior = new ValidationBehavior<SampleCommand, Result>(new[] { validatorMock.Object });
        var command = new SampleCommand("");
        var nextCalled = false;
        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Data" && e.Description == "Data is required");
    }

    [Fact]
    public async Task Handle_WhenValidationFailsAndResponseIsResultOfT_ShouldReturnTypedFailure()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<SampleGenericCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleGenericCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Data", "Invalid generic data") }));

        var behavior = new ValidationBehavior<SampleGenericCommand, Result<string>>(new[] { validatorMock.Object });
        var command = new SampleGenericCommand("");
        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result<string>.Success("OK"));
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Data" && e.Description == "Invalid generic data");
    }

    [Fact]
    public async Task Handle_WhenValidationFailsAndResponseIsNotResult_ShouldThrowValidationException()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<SamplePrimitiveCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SamplePrimitiveCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Data", "Invalid primitive data") }));

        var behavior = new ValidationBehavior<SamplePrimitiveCommand, string>(new[] { validatorMock.Object });
        var command = new SamplePrimitiveCommand("");
        RequestHandlerDelegate<string> next = () => Task.FromResult("OK");

        // Act
        var act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
