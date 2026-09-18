using OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeLanguage;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.Employees;

public class UpdateEmployeeLanguageCommandValidatorTests
{
    private readonly UpdateEmployeeLanguageCommandValidator _validator;

    public UpdateEmployeeLanguageCommandValidatorTests()
    {
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _validator = new UpdateEmployeeLanguageCommandValidator(localizer);
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPassValidation()
    {
        var command = new UpdateEmployeeLanguageCommand(Guid.NewGuid(), LanguageCode.Fr);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = new UpdateEmployeeLanguageCommand(Guid.Empty, LanguageCode.Fr);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeLanguageCommand.EmployeeId));
    }

    [Fact]
    public void Validate_WithInvalidLanguage_ShouldHaveValidationError()
    {
        var command = new UpdateEmployeeLanguageCommand(Guid.NewGuid(), (LanguageCode)999);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeLanguageCommand.Language));
    }
}
