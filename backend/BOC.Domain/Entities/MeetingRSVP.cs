using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class MeetingRSVP : Entity
{
    public Guid MeetingId { get; set; }
    public Guid MemberId { get; set; }
    public string Response { get; set; } = string.Empty; // Accept, Decline
    public string Justification { get; set; } = string.Empty;
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
    public virtual AppUser Member { get; set; } = null!;
}
