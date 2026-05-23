using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class MeetingAttendance : Entity
{
    public Guid MeetingId { get; set; }
    public Guid MemberId { get; set; }
    public bool Attended { get; set; }
    public DateTime? AttendanceMarkedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
    public virtual AppUser Member { get; set; } = null!;
}
