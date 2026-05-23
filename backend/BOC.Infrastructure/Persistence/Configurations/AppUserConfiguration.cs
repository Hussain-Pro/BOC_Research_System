using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUsers");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(u => u.EmployeeID)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.NationalID)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.NormalizedEmail)
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        builder.Property(u => u.SecurityStamp)
            .HasMaxLength(500);

        builder.Property(u => u.ConcurrencyStamp)
            .HasMaxLength(500);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(u => u.EvaluatorStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(BOC.Domain.Enums.EvaluatorStatus.Active);

        builder.Property(u => u.AccountStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(BOC.Domain.Enums.AccountStatus.Pending_HR_Verification);

        builder.Property(u => u.RefreshTokenHash)
            .HasMaxLength(500);

        builder.Property(u => u.TwoFactorSecret)
            .HasMaxLength(500);

        builder.Property(u => u.DeviceFingerprint)
            .HasMaxLength(500);

        // Unique indexes
        builder.HasIndex(u => u.EmployeeID).IsUnique();
        builder.HasIndex(u => u.NationalID).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        // Foreign Key Relationships
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Directorate)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DirectorateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
