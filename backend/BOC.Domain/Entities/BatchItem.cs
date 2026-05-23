using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class BatchItem : Entity
{
    public Guid BatchId { get; set; }
    public Guid ResearchId { get; set; }
    public string? MinistryDecision { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual MinistryBatch Batch { get; set; } = null!;
    public virtual ResearchPaper ResearchPaper { get; set; } = null!;
}
