using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class ResearchCorrection : Entity
{
    public Guid ResearchId { get; set; }
    public int CorrectionRound { get; set; } = 1;
    public string DocumentPath { get; set; } = string.Empty;
    public string? SecretaryNotes { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ResearchPaper ResearchPaper { get; set; } = null!;
}
