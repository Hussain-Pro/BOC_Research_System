using System;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class Vote : Entity
{
    public Guid MeetingId { get; set; }
    public Guid ResearchId { get; set; }
    public Guid MemberId { get; set; }
    public VoteValue VoteValue { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
    public virtual ResearchPaper ResearchPaper { get; set; } = null!;
    public virtual AppUser Member { get; set; } = null!;
}
