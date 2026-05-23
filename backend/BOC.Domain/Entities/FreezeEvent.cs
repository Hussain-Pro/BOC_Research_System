using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class FreezeEvent : Entity
{
    public Guid MeetingMinutesId { get; set; }
    public Guid FrozenById { get; set; }
    public DateTime FrozenAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual MeetingMinutes MeetingMinutes { get; set; } = null!;
    public virtual AppUser FrozenBy { get; set; } = null!;
}
