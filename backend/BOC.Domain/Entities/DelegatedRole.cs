using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class DelegatedRole : Entity
{
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public long PermissionsMask { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual AppUser FromUser { get; set; } = null!;
    public virtual AppUser ToUser { get; set; } = null!;
    public virtual AppUser CreatedBy { get; set; } = null!;
}
