using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Configurations;

public class HRVerificationQueueConfiguration : IEntityTypeConfiguration<HRVerificationQueue>
{
    public void Configure(EntityTypeBuilder<HRVerificationQueue> builder)
    {
        builder.ToTable("HRVerificationQueue");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(h => h.EmployeeID)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.NationalID)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(BOC.Domain.Enums.HRVerificationStatus.Pending);

        builder.Property(h => h.RejectionReason)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.VerifiedBy)
            .WithMany()
            .HasForeignKey(h => h.VerifiedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
