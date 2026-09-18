using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenX.Gest.Domain.Entities;

namespace OpenX.Gest.Infrastructure.Persistence.Configurations;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("TimeEntries");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.EmployeeId)
            .IsRequired();

        builder.Property(t => t.ClockInUtc)
            .IsRequired();

        builder.Property(t => t.ClockOutUtc);

        builder.Property(t => t.BreakDurationMinutes)
            .IsRequired();

        // OwnsOne per le coordinate GPS puntuali (Art. 26 OLL 3)
        builder.OwnsOne(t => t.PunctualClockInGps, gps =>
        {
            gps.Property(p => p.Latitude).HasColumnName("ClockInLatitude");
            gps.Property(p => p.Longitude).HasColumnName("ClockInLongitude");
            gps.Property(p => p.AccuracyMeters).HasColumnName("ClockInAccuracyMeters");
            gps.Property(p => p.TimestampUtc).HasColumnName("ClockInGpsTimestampUtc");
        });

        builder.OwnsOne(t => t.PunctualClockOutGps, gps =>
        {
            gps.Property(p => p.Latitude).HasColumnName("ClockOutLatitude");
            gps.Property(p => p.Longitude).HasColumnName("ClockOutLongitude");
            gps.Property(p => p.AccuracyMeters).HasColumnName("ClockOutAccuracyMeters");
            gps.Property(p => p.TimestampUtc).HasColumnName("ClockOutGpsTimestampUtc");
        });

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.Violations)
            .IsRequired();

        builder.HasMany(t => t.AuditTrail)
            .WithOne()
            .HasForeignKey(a => a.TimeEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.EmployeeId, t.ClockInUtc });
    }
}
