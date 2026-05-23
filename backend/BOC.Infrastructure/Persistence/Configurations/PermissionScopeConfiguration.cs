using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API mapping for PermissionScope.
/// Blueprint Reference: Section 2.3 — PermissionScopes table.
/// Scope types: "Directorate", "Department", "Role", "All".
/// </summary>
public class PermissionScopeConfiguration : IEntityTypeConfiguration<PermissionScope>
{
    public void Configure(EntityTypeBuilder<PermissionScope> builder)
    {
        builder.ToTable("PermissionScopes");

        builder.HasKey(ps => ps.Id);
        builder.Property(ps => ps.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(ps => ps.PermissionId)
            .IsRequired();

        builder.Property(ps => ps.ScopeType)
            .IsRequired()
            .HasMaxLength(50);  // "Directorate" | "Department" | "Role" | "All"

        builder.Property(ps => ps.ScopeValue)
            .IsRequired()
            .HasMaxLength(200); // Guid string or wildcard

        builder.Property(ps => ps.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Composite index — query: "what scopes apply to this permission?"
        builder.HasIndex(ps => new { ps.PermissionId, ps.ScopeType })
            .HasDatabaseName("IX_PermissionScopes_PermissionId_ScopeType");
    }
}
