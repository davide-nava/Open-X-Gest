using OpenX.Gest.Application.Features.TimeTracking.Commands.CorrectTimeEntry;
using OpenX.Gest.Application.UnitTests.Common;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class CorrectTimeEntryCommandValidatorTests
{
    private readonly CorrectTimeEntryCommandValidator _validator;

    public CorrectTimeEntryCommandValidatorTests()
    {
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _validator = new CorrectTimeEntryCommandValidator(localizer);
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPassValidation()
    {
        var command = new CorrectTimeEntryCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow,
            30,
            "Motivazione valida di rettifica");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyTimeEntryId_ShouldHaveValidationError()
    {
        var command = new CorrectTimeEntryCommand(
            Guid.Empty,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow,
            30,
            "Motivazione valida");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CorrectTimeEntryCommand.TimeEntryId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Abc")] // Less than 5 characters
    public void Validate_WithShortOrEmptyReason_ShouldHaveValidationError(string invalidReason)
    {
        var command = new CorrectTimeEntryCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow,
            30,
            invalidReason);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CorrectTimeEntryCommand.MandatoryReason));
    }

    [Fact]
    public void Validate_WithNegativeBreak_ShouldHaveValidationError()
    {
        var command = new CorrectTimeEntryCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow,
            -10,
            "Motivazione valida");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CorrectTimeEntryCommand.NewBreakMinutes));
    }
}
