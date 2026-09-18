using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Entities;

namespace OpenX.Gest.Infrastructure.Persistence;

public class OpenXGestDbContext : DbContext
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<TimeCorrectionAudit> TimeCorrectionAudits => Set<TimeCorrectionAudit>();

    public OpenXGestDbContext(DbContextOptions<OpenXGestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applica le configurazioni Fluent API
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Aggiunge shadow properties per tutte le entità che derivano da BaseEntity
        foreach (var clrType in modelBuilder.Model.GetEntityTypes()
                     .Select(t => t.ClrType)
                     .Where(t => typeof(BaseEntity).IsAssignableFrom(t)))
        {
            modelBuilder.Entity(clrType)
                .Property<DateTime>("CreatedAt")
                .IsRequired();

            modelBuilder.Entity(clrType)
                .Property<string>("CreatedBy")
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity(clrType)
                .Property<DateTime?>("LastModifiedAt");

            modelBuilder.Entity(clrType)
                .Property<string?>("LastModifiedBy")
                .HasMaxLength(255);
        }
    }
}
