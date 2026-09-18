using OpenX.Gest.Application.Features.TimeTracking.Queries.GetTimesheet;
using OpenX.Gest.Application.UnitTests.Common;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class GetTimesheetQueryValidatorTests
{
    private readonly GetTimesheetQueryValidator _validator;

    public GetTimesheetQueryValidatorTests()
    {
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _validator = new GetTimesheetQueryValidator(localizer);
    }

    [Fact]
    public void Validate_WithValidRange_ShouldPassValidation()
    {
        var query = new GetTimesheetQuery(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var query = new GetTimesheetQuery(
            Guid.Empty,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTimesheetQuery.EmployeeId));
    }

    [Fact]
    public void Validate_WithEndDateBeforeStartDate_ShouldHaveValidationError()
    {
        var query = new GetTimesheetQuery(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(-1));

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTimesheetQuery.EndDateUtc));
    }
}
