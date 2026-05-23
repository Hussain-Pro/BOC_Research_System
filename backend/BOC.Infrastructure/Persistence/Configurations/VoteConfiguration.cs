using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Configurations;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.ToTable("Votes");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(v => v.VoteValue)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(v => v.Meeting)
            .WithMany(m => m.Votes)
            .HasForeignKey(v => v.MeetingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.ResearchPaper)
            .WithMany(r => r.Votes)
            .HasForeignKey(v => v.ResearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Member)
            .WithMany()
            .HasForeignKey(v => v.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
