using Microsoft.EntityFrameworkCore;
using Moq;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;
using OpenX.Gest.Infrastructure.Persistence;
using OpenX.Gest.Infrastructure.Persistence.Interceptors;

namespace OpenX.Gest.Application.UnitTests.Infrastructure;

public class AuditLogInterceptorTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly AuditLogInterceptor _interceptor;

    public AuditLogInterceptorTests()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _currentUserMock.Setup(u => u.Email).Returns("hr.admin@openx.ch");
        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc));

        _interceptor = new AuditLogInterceptor(_currentUserMock.Object, _dateTimeProviderMock.Object);
    }

    private OpenXGestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OpenXGestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        return new OpenXGestDbContext(options);
    }

    [Fact]
    public async Task SaveChanges_WhenAddingEntity_ShouldPopulateShadowProperties()
    {
        // Arrange
        using var context = CreateDbContext();
        var employee = new Employee(
            Guid.NewGuid(),
            "Elena",
            "Corti",
            "elena.corti@openx.ch",
            "HR",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.StandardRecord,
            LanguageCode.It);

        context.Employees.Add(employee);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var entry = context.Entry(employee);
        entry.Property("CreatedAt").CurrentValue.Should().Be(new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc));
        entry.Property("CreatedBy").CurrentValue.Should().Be("hr.admin@openx.ch");
    }

    [Fact]
    public async Task SaveChanges_WhenDeletingTimeEntry_ShouldThrowInvalidOperationExceptionUnderSwissLaw()
    {
        // Arrange
        using var context = CreateDbContext();
        var timeEntry = new TimeEntry(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        context.TimeEntries.Add(timeEntry);
        await context.SaveChangesAsync();

        // Act
        context.TimeEntries.Remove(timeEntry);
        var act = async () => await context.SaveChangesAsync();

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("*Art. 73 cpv. 2 OLL 1*");
    }

    [Fact]
    public async Task SaveChanges_WhenDeletingTimeCorrectionAudit_ShouldThrowInvalidOperationExceptionUnderSwissLaw()
    {
        // Arrange
        using var context = CreateDbContext();
        var audit = new TimeCorrectionAudit(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow,
            30,
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow,
            30,
            "Rettifica legale");

        context.TimeCorrectionAudits.Add(audit);
        await context.SaveChangesAsync();

        // Act
        context.TimeCorrectionAudits.Remove(audit);
        var act = async () => await context.SaveChangesAsync();

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("*Art. 73 cpv. 2 OLL 1*");
    }
}
