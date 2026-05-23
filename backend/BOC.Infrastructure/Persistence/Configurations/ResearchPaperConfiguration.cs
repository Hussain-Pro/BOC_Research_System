using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Configurations;

public class ResearchPaperConfiguration : IEntityTypeConfiguration<ResearchPaper>
{
    public void Configure(EntityTypeBuilder<ResearchPaper> builder)
    {
        builder.ToTable("ResearchPapers");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(r => r.TrackingNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.State)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(BOC.Domain.Enums.ResearchState.Draft);

        builder.Property(r => r.FinalScore)
            .HasPrecision(5, 2);

        builder.Property(r => r.IsArchived)
            .HasComputedColumnSql("CASE WHEN [State] = N'Archived' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", stored: true);

        builder.Property(r => r.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(r => r.TrackingNumber).IsUnique();

        // Foreign Key Relationships
        builder.HasOne(r => r.Researcher)
            .WithMany(u => u.Submissions)
            .HasForeignKey(r => r.ResearcherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Category)
            .WithMany(c => c.ResearchPapers)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReplacedResearch)
            .WithMany(r => r.ReplacedByResearchPapers)
            .HasForeignKey(r => r.ReplacedResearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Meeting)
            .WithMany(m => m.ResearchPapers)
            .HasForeignKey(r => r.MeetingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.MeetingMinutes)
            .WithMany(m => m.ResearchPapers)
            .HasForeignKey(r => r.MeetingMinutesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Department)
            .WithMany(d => d.ResearchPapers)
            .HasForeignKey(r => r.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Directorate)
            .WithMany(d => d.ResearchPapers)
            .HasForeignKey(r => r.DirectorateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
