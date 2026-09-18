using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenX.Gest.Domain.Entities;

namespace OpenX.Gest.Infrastructure.Persistence.Configurations;

public class TimeCorrectionAuditConfiguration : IEntityTypeConfiguration<TimeCorrectionAudit>
{
    public void Configure(EntityTypeBuilder<TimeCorrectionAudit> builder)
    {
        builder.ToTable("TimeCorrectionAudits");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TimeEntryId)
            .IsRequired();

        builder.Property(a => a.OperatorId)
            .IsRequired();

        builder.Property(a => a.TimestampUtc)
            .IsRequired();

        builder.Property(a => a.PreCorrectionClockInUtc)
            .IsRequired();

        builder.Property(a => a.PostCorrectionClockInUtc)
            .IsRequired();

        builder.Property(a => a.MandatoryReason)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.HasIndex(a => a.TimeEntryId);
    }
}
