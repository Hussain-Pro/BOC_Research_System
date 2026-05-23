using System;
using System.Collections.Generic;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class ResearchCategory : Entity
{
    public string Name { get; set; } = string.Empty;
    public Guid SpecializationId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Specialization Specialization { get; set; } = null!;
    public virtual ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
}
