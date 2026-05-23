using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Configurations;

public class DelegatedRoleConfiguration : IEntityTypeConfiguration<DelegatedRole>
{
    public void Configure(EntityTypeBuilder<DelegatedRole> builder)
    {
        builder.ToTable("DelegatedRoles");

        builder.HasKey(dr => dr.Id);
        builder.Property(dr => dr.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        // Relationships
        builder.HasOne(dr => dr.FromUser)
            .WithMany(u => u.DelegatedFromRoles)
            .HasForeignKey(dr => dr.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dr => dr.ToUser)
            .WithMany(u => u.DelegatedToRoles)
            .HasForeignKey(dr => dr.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dr => dr.CreatedBy)
            .WithMany()
            .HasForeignKey(dr => dr.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
