using OpenX.Gest.Application.Features.TimeTracking.Commands.ClockOut;
using OpenX.Gest.Application.UnitTests.Common;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class ClockOutCommandValidatorTests
{
    private readonly ClockOutCommandValidator _validator;

    public ClockOutCommandValidatorTests()
    {
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _validator = new ClockOutCommandValidator(localizer);
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPassValidation()
    {
        var command = new ClockOutCommand(Guid.NewGuid(), 30);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = new ClockOutCommand(Guid.Empty, 30);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ClockOutCommand.EmployeeId));
    }

    [Fact]
    public void Validate_WithNegativeBreakDuration_ShouldHaveValidationError()
    {
        var command = new ClockOutCommand(Guid.NewGuid(), -15);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ClockOutCommand.BreakDurationMinutes));
    }
}
