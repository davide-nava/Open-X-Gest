using OpenX.Gest.Application.Features.Employees.Queries.GetEmployeeById;
using OpenX.Gest.Application.UnitTests.Common;

namespace OpenX.Gest.Application.UnitTests.Features.Employees;

public class GetEmployeeByIdQueryValidatorTests
{
    private readonly GetEmployeeByIdQueryValidator _validator;

    public GetEmployeeByIdQueryValidatorTests()
    {
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _validator = new GetEmployeeByIdQueryValidator(localizer);
    }

    [Fact]
    public void Validate_WithValidId_ShouldPassValidation()
    {
        var query = new GetEmployeeByIdQuery(Guid.NewGuid());
        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveValidationError()
    {
        var query = new GetEmployeeByIdQuery(Guid.Empty);
        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetEmployeeByIdQuery.Id));
    }
}
