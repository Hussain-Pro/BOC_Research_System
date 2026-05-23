using System;
using System.Collections.Generic;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class PlagiarismLockout : Entity
{
    public Guid ResearchId { get; set; }
    public DateTime LockedUntil { get; set; }
    public string? Reason { get; set; }
    public Guid DetectedById { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ResearchPaper ResearchPaper { get; set; } = null!;
    public virtual AppUser DetectedBy { get; set; } = null!;
    public virtual ICollection<PlagiarismOverrideJustification> OverrideJustifications { get; set; } = new List<PlagiarismOverrideJustification>();
}
