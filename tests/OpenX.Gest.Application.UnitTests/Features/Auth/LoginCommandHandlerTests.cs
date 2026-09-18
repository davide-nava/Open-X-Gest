using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.Auth.Commands;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _jwtServiceMock = new Mock<IJwtTokenService>();
        var localizer = TestLocalizerHelper.CreateMockLocalizer();

        _handler = new LoginCommandHandler(
            _employeeRepoMock.Object,
            _jwtServiceMock.Object,
            localizer);
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldReturnUnauthorizedFailure()
    {
        // Arrange
        _employeeRepoMock
            .Setup(r => r.GetByEmailAsync("unknown@openx.ch", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var command = new LoginCommand("unknown@openx.ch", "SecretPassword123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        result.Error.Type.Should().Be(OpenX.Gest.Domain.Common.ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_WithStandardEmployee_ShouldReturnSuccessWithEmployeeRole()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Mario",
            "Rossi",
            "mario.rossi@openx.ch",
            "Development",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        _employeeRepoMock
            .Setup(r => r.GetByEmailAsync("mario.rossi@openx.ch", It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _jwtServiceMock
            .Setup(j => j.GenerateToken(employee, "Employee"))
            .Returns("sample.jwt.token");

        var command = new LoginCommand("mario.rossi@openx.ch", "ValidPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("sample.jwt.token");
        result.Value.FullName.Should().Be("Mario Rossi");
        result.Value.Role.Should().Be("Employee");
        result.Value.PreferredLanguage.Should().Be(LanguageCode.It);
        result.Value.Oll1Regime.Should().Be(Oll1Regime.StandardRecord);
    }

    [Theory]
    [InlineData("admin@openx.ch")]
    [InlineData("hr.manager@openx.ch")]
    public async Task Handle_WithAdminOrHrEmail_ShouldAssignHRManagerRole(string adminEmail)
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Admin",
            "User",
            adminEmail,
            "HR",
            42.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.En);

        _employeeRepoMock
            .Setup(r => r.GetByEmailAsync(adminEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _jwtServiceMock
            .Setup(j => j.GenerateToken(employee, "HRManager"))
            .Returns("admin.jwt.token");

        var command = new LoginCommand(adminEmail, "AdminPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("HRManager");
        result.Value.Token.Should().Be("admin.jwt.token");
    }
}
