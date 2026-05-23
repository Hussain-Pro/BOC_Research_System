using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Configurations;

public class TriageMappingConfiguration : IEntityTypeConfiguration<TriageMapping>
{
    public void Configure(EntityTypeBuilder<TriageMapping> builder)
    {
        builder.ToTable("TriageMappings");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(t => t.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Relationships
        builder.HasOne(t => t.ResearchPaper)
            .WithMany(r => r.TriageMappings)
            .HasForeignKey(t => t.ResearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.MappedBy)
            .WithMany()
            .HasForeignKey(t => t.MappedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Evaluator)
            .WithMany()
            .HasForeignKey(t => t.EvaluatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Member)
            .WithMany()
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
