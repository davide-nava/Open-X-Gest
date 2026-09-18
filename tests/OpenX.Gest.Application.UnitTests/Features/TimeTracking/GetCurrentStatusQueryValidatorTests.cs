using OpenX.Gest.Application.Features.TimeTracking.Queries.GetCurrentStatus;
using OpenX.Gest.Application.UnitTests.Common;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class GetCurrentStatusQueryValidatorTests
{
    private readonly GetCurrentStatusQueryValidator _validator;

    public GetCurrentStatusQueryValidatorTests()
    {
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _validator = new GetCurrentStatusQueryValidator(localizer);
    }

    [Fact]
    public void Validate_WithValidEmployeeId_ShouldPassValidation()
    {
        var query = new GetCurrentStatusQuery(Guid.NewGuid());
        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var query = new GetCurrentStatusQuery(Guid.Empty);
        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetCurrentStatusQuery.EmployeeId));
    }
}
