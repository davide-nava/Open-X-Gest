using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenX.Gest.Domain.Entities;

namespace OpenX.Gest.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Department)
            .HasMaxLength(100);

        builder.Property(e => e.ContractualWeeklyHours)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(e => e.StatutoryWeeklyLimit)
            .IsRequired();

        builder.Property(e => e.Oll1Regime)
            .IsRequired();

        builder.Property(e => e.PreferredLanguage)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();
    }
}
