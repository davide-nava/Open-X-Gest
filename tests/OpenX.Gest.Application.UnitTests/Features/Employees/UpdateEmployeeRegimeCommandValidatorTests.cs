using OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeRegime;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.Employees;

public class UpdateEmployeeRegimeCommandValidatorTests
{
    private readonly UpdateEmployeeRegimeCommandValidator _validator;

    public UpdateEmployeeRegimeCommandValidatorTests()
    {
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _validator = new UpdateEmployeeRegimeCommandValidator(localizer);
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPassValidation()
    {
        var command = new UpdateEmployeeRegimeCommand(Guid.NewGuid(), Oll1Regime.StandardRecord);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = new UpdateEmployeeRegimeCommand(Guid.Empty, Oll1Regime.StandardRecord);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRegimeCommand.EmployeeId));
    }

    [Fact]
    public void Validate_WithInvalidRegime_ShouldHaveValidationError()
    {
        var command = new UpdateEmployeeRegimeCommand(Guid.NewGuid(), (Oll1Regime)999);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRegimeCommand.Regime));
    }
}
