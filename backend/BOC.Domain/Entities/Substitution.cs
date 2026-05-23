using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class Substitution : Entity
{
    public Guid OriginalResearchId { get; set; }
    public Guid NewResearchId { get; set; }
    public Guid SubstitutedById { get; set; }
    public string? Justification { get; set; }
    public DateTime SubstitutedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ResearchPaper OriginalResearch { get; set; } = null!;
    public virtual ResearchPaper NewResearch { get; set; } = null!;
    public virtual AppUser SubstitutedBy { get; set; } = null!;
}
