using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class PlagiarismOverrideJustification : Entity
{
    public Guid LockoutId { get; set; }
    public Guid LiftedById { get; set; }
    public string JustificationDocumentPath { get; set; } = string.Empty;
    public DateTime LiftedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual PlagiarismLockout Lockout { get; set; } = null!;
    public virtual AppUser LiftedBy { get; set; } = null!;
}
