using OpenX.Gest.Application.Features.TimeTracking.Queries.ExportSecoReport;
using OpenX.Gest.Application.UnitTests.Common;

namespace OpenX.Gest.Application.UnitTests.Features.TimeTracking;

public class ExportSecoReportQueryValidatorTests
{
    private readonly ExportSecoReportQueryValidator _validator;

    public ExportSecoReportQueryValidatorTests()
    {
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _validator = new ExportSecoReportQueryValidator(localizer);
    }

    [Fact]
    public void Validate_WithValidQuery_ShouldPassValidation()
    {
        var query = new ExportSecoReportQuery(Guid.NewGuid(), 2026, 3, "csv", "it");
        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var query = new ExportSecoReportQuery(Guid.Empty, 2026, 3);
        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ExportSecoReportQuery.EmployeeId));
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void Validate_WithOutOfRangeYear_ShouldHaveValidationError(int year)
    {
        var query = new ExportSecoReportQuery(Guid.NewGuid(), year, 5);
        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ExportSecoReportQuery.Year));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Validate_WithOutOfRangeMonth_ShouldHaveValidationError(int month)
    {
        var query = new ExportSecoReportQuery(Guid.NewGuid(), 2026, month);
        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ExportSecoReportQuery.Month));
    }
}
