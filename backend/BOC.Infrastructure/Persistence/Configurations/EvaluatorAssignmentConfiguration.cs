using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Configurations;

public class EvaluatorAssignmentConfiguration : IEntityTypeConfiguration<EvaluatorAssignment>
{
    public void Configure(EntityTypeBuilder<EvaluatorAssignment> builder)
    {
        builder.ToTable("EvaluatorAssignments");

        builder.HasKey(ea => ea.Id);
        builder.Property(ea => ea.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(ea => ea.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(BOC.Domain.Enums.AssignmentStatus.Active);

        builder.Property(ea => ea.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Foreign Key Relationships
        builder.HasOne(ea => ea.ResearchPaper)
            .WithMany(r => r.EvaluatorAssignments)
            .HasForeignKey(ea => ea.ResearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ea => ea.Evaluator)
            .WithMany(u => u.Assignments)
            .HasForeignKey(ea => ea.EvaluatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ea => ea.AssignedBy)
            .WithMany()
            .HasForeignKey(ea => ea.AssignedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
