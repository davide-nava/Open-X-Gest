using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.Employees.Queries.GetEmployees;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.UnitTests.Features.Employees;

public class GetEmployeesQueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly GetEmployeesQueryHandler _handler;

    public GetEmployeesQueryHandlerTests()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _handler = new GetEmployeesQueryHandler(_employeeRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmployeesExist_ShouldReturnListOfEmployeeDtos()
    {
        // Arrange
        var list = new List<Employee>
        {
            new(Guid.NewGuid(), "Anna", "Keller", "anna.keller@openx.ch", "Engineering", 42.0m, StatutoryWeeklyLimit.Hours45, Oll1Regime.StandardRecord, LanguageCode.De),
            new(Guid.NewGuid(), "Luca", "Bernasconi", "luca.b@openx.ch", "Marketing", 40.0m, StatutoryWeeklyLimit.Hours45, Oll1Regime.SimplifiedRecord, LanguageCode.It)
        };

        _employeeRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var query = new GetEmployeesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].FirstName.Should().Be("Anna");
        result.Value[1].FirstName.Should().Be("Luca");
    }

    [Fact]
    public async Task Handle_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        _employeeRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        var query = new GetEmployeesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
