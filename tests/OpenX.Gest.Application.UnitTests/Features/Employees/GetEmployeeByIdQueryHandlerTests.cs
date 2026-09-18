using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.Employees.Queries.GetEmployeeById;
using OpenX.Gest.Application.UnitTests.Common;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.Employees;

public class GetEmployeeByIdQueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly GetEmployeeByIdQueryHandler _handler;

    public GetEmployeeByIdQueryHandlerTests()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        var localizer = TestLocalizerHelper.CreateMockLocalizer();
        _handler = new GetEmployeeByIdQueryHandler(_employeeRepoMock.Object, localizer);
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var query = new GetEmployeeByIdQuery(id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
    }

    [Fact]
    public async Task Handle_WhenEmployeeFound_ShouldReturnEmployeeDto()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "Clara",
            "Müller",
            "clara.m@openx.ch",
            "Legal",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.De);

        _employeeRepoMock
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var query = new GetEmployeeByIdQuery(employee.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(employee.Id);
        result.Value.FirstName.Should().Be("Clara");
        result.Value.LastName.Should().Be("Müller");
        result.Value.Email.Should().Be("clara.m@openx.ch");
    }
}
