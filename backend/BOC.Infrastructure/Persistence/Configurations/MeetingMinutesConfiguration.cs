using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Configurations;

public class MeetingMinutesConfiguration : IEntityTypeConfiguration<MeetingMinutes>
{
    public void Configure(EntityTypeBuilder<MeetingMinutes> builder)
    {
        builder.ToTable("MeetingMinutes");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(m => m.MinutesNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(BOC.Domain.Enums.MeetingMinutesStatus.Draft);

        builder.Property(m => m.IsFrozen)
            .HasComputedColumnSql("CASE WHEN [Status] = N'Minutes_Frozen' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", stored: true);

        builder.Property(m => m.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(m => m.MinutesNumber).IsUnique();

        // Foreign Key Relationships
        builder.HasOne(m => m.Meeting)
            .WithMany(m => m.Minutes)
            .HasForeignKey(m => m.MeetingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.SignedBy)
            .WithMany()
            .HasForeignKey(m => m.SignedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
