using System;
using System.Collections.Generic;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class Directorate : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public virtual ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
}
