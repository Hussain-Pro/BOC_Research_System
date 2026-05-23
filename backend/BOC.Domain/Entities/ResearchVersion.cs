using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class ResearchVersion : Entity
{
    public Guid ResearchId { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string DocumentPath { get; set; } = string.Empty;
    public string? ChangeSummary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ResearchPaper ResearchPaper { get; set; } = null!;
}
