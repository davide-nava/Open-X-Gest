using OpenX.Gest.Application.Features.TimeTracking.Commands.ClockIn;
using OpenX.Gest.Application.UnitTests.Common;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class ClockInCommandValidatorTests
{
    private readonly ClockInCommandValidator _validator;

    public ClockInCommandValidatorTests()
    {
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _validator = new ClockInCommandValidator(localizer);
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPassValidation()
    {
        var command = new ClockInCommand(Guid.NewGuid(), 46.0037, 8.9511);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = new ClockInCommand(Guid.Empty);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ClockInCommand.EmployeeId));
    }
}
