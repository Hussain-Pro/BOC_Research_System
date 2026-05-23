using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class ChairmanScore : Entity
{
    public Guid ResearchId { get; set; }
    public Guid ChairmanId { get; set; }
    public decimal Score { get; set; }
    public Guid MeetingMinutesId { get; set; }
    public string? Comments { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ResearchPaper ResearchPaper { get; set; } = null!;
    public virtual AppUser Chairman { get; set; } = null!;
    public virtual MeetingMinutes MeetingMinutes { get; set; } = null!;
}
