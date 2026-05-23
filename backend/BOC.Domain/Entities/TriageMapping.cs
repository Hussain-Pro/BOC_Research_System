using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class TriageMapping : Entity
{
    public Guid ResearchId { get; set; }
    public Guid MappedById { get; set; }
    public Guid? EvaluatorId { get; set; }
    public Guid? MemberId { get; set; }
    public bool IsFinalized { get; set; } = false;
    public DateTime MappedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation properties
    public virtual ResearchPaper ResearchPaper { get; set; } = null!;
    public virtual AppUser MappedBy { get; set; } = null!;
    public virtual AppUser? Evaluator { get; set; }
    public virtual AppUser? Member { get; set; }
}
