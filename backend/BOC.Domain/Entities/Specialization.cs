using System;
using System.Collections.Generic;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class Specialization : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<EvaluatorSpecialization> EvaluatorSpecializations { get; set; } = new List<EvaluatorSpecialization>();
    public virtual ICollection<ResearchCategory> ResearchCategories { get; set; } = new List<ResearchCategory>();
}
