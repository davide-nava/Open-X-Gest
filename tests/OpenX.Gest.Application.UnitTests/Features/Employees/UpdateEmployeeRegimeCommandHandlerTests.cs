using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeRegime;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.Employees;

public class UpdateEmployeeRegimeCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateEmployeeRegimeCommandHandler _handler;

    public UpdateEmployeeRegimeCommandHandlerTests()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var localizer = TestLocalizerHelper.CreateMockLocalizer();

        _handler = new UpdateEmployeeRegimeCommandHandler(
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

        var command = new UpdateEmployeeRegimeCommand(id, Oll1Regime.SimplifiedRecord);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmployeeExists_ShouldUpdateRegimeAndSaveChanges()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Gianni",
            "Bianchi",
            "gianni.bianchi@openx.ch",
            "Sales",
            42.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var command = new UpdateEmployeeRegimeCommand(employee.Id, Oll1Regime.OptOut);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        employee.Oll1Regime.Should().Be(Oll1Regime.OptOut);
        _employeeRepoMock.Verify(r => r.Update(employee), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
