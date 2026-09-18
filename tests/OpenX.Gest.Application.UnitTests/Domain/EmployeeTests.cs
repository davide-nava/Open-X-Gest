using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Domain;

public class EmployeeTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiateEmployeeCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var employee = new Employee(
            id,
            "Mario",
            "Rossi",
            "mario.rossi@openx.ch",
            "IT",
            42.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        // Assert
        employee.Id.Should().Be(id);
        employee.FirstName.Should().Be("Mario");
        employee.LastName.Should().Be("Rossi");
        employee.Email.Should().Be("mario.rossi@openx.ch");
        employee.Department.Should().Be("IT");
        employee.ContractualWeeklyHours.Should().Be(42.0m);
        employee.StatutoryWeeklyLimit.Should().Be(StatutoryWeeklyLimit.Hours45);
        employee.Oll1Regime.Should().Be(Oll1Regime.StandardRecord);
        employee.PreferredLanguage.Should().Be(LanguageCode.It);
        employee.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldGenerateNewId()
    {
        // Act
        var employee = new Employee(
            Guid.Empty,
            "Anna",
            "Bianchi",
            "anna.bianchi@openx.ch",
            "HR",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.En);

        // Assert
        employee.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidFirstName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        var act = () => new Employee(
            Guid.NewGuid(),
            invalidName,
            "Rossi",
            "test@openx.ch",
            "Dept",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("firstName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidLastName_ShouldThrowArgumentException(string? invalidLastName)
    {
        // Act
        var act = () => new Employee(
            Guid.NewGuid(),
            "Mario",
            invalidLastName,
            "test@openx.ch",
            "Dept",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("lastName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act
        var act = () => new Employee(
            Guid.NewGuid(),
            "Mario",
            "Rossi",
            invalidEmail,
            "Dept",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("email");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_WithZeroOrNegativeWeeklyHours_ShouldDefaultTo40(decimal invalidHours)
    {
        // Act
        var employee = new Employee(
            Guid.NewGuid(),
            "Luca",
            "Verdi",
            "luca.verdi@openx.ch",
            "Finance",
            invalidHours,
            StatutoryWeeklyLimit.Hours50,
            Oll1Regime.SimplifiedRecord,
            LanguageCode.De);

        // Assert
        employee.ContractualWeeklyHours.Should().Be(40.0m);
    }

    [Fact]
    public void UpdateContractualTerms_WithValidHours_ShouldUpdateSuccessfully()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Mario",
            "Rossi",
            "mario@openx.ch",
            "IT",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        // Act
        employee.UpdateContractualTerms(42.5m, StatutoryWeeklyLimit.Hours50);

        // Assert
        employee.ContractualWeeklyHours.Should().Be(42.5m);
        employee.StatutoryWeeklyLimit.Should().Be(StatutoryWeeklyLimit.Hours50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(61)]
    public void UpdateContractualTerms_WithOutOfRangeHours_ShouldThrowArgumentOutOfRangeException(decimal outOfRangeHours)
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Mario",
            "Rossi",
            "mario@openx.ch",
            "IT",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        // Act
        var act = () => employee.UpdateContractualTerms(outOfRangeHours, StatutoryWeeklyLimit.Hours45);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("weeklyHours");
    }

    [Fact]
    public void UpdateOll1Regime_ShouldUpdateRegime()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Mario",
            "Rossi",
            "mario@openx.ch",
            "IT",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        // Act
        employee.UpdateOll1Regime(Oll1Regime.OptOut);

        // Assert
        employee.Oll1Regime.Should().Be(Oll1Regime.OptOut);
    }

    [Fact]
    public void SetPreferredLanguage_ShouldUpdateLanguage()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Mario",
            "Rossi",
            "mario@openx.ch",
            "IT",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        // Act
        employee.SetPreferredLanguage(LanguageCode.Fr);

        // Assert
        employee.PreferredLanguage.Should().Be(LanguageCode.Fr);
    }

    [Fact]
    public void ActivateAndDeactivate_ShouldToggleIsActive()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Mario",
            "Rossi",
            "mario@openx.ch",
            "IT",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        // Act & Assert
        employee.Deactivate();
        employee.IsActive.Should().BeFalse();

        employee.Activate();
        employee.IsActive.Should().BeTrue();
    }
}
