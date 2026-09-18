using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeLanguage;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.Employees;

public class UpdateEmployeeLanguageCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateEmployeeLanguageCommandHandler _handler;

    public UpdateEmployeeLanguageCommandHandlerTests()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var localizer = TestLocalizerHelper.CreateMockLocalizer();

        _handler = new UpdateEmployeeLanguageCommandHandler(
            _employeeRepoMock.Object,
            _unitOfWorkMock.Object,
            localizer);
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var command = new UpdateEmployeeLanguageCommand(id, LanguageCode.De);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmployeeExists_ShouldUpdateLanguageAndSaveChanges()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Sophie",
            "Martin",
            "sophie.martin@openx.ch",
            "Finance",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.Fr);

        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var command = new UpdateEmployeeLanguageCommand(employee.Id, LanguageCode.De);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        employee.PreferredLanguage.Should().Be(LanguageCode.De);
        _employeeRepoMock.Verify(r => r.Update(employee), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
