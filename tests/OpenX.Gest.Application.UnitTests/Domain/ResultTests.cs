using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.UnitTests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        var error = Error.Validation("Code.Test", "Test validation error");
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void GenericResult_Success_ShouldHoldValue()
    {
        var result = Result<string>.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void GenericResult_Failure_AccessingValueShouldThrowInvalidOperationException()
    {
        var result = Result<string>.Failure(Error.NotFound("Not.Found", "Not found"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertValueToSuccessResult()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldStoreAllErrors()
    {
        var err1 = Error.Validation("Err1", "Desc1");
        var err2 = Error.Validation("Err2", "Desc2");

        var result = Result.Failure(new[] { err1, err2 });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Error.Should().Be(err1);
    }
}
