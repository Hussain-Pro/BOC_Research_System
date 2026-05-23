using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class MeetingAgenda : Entity
{
    public Guid MeetingId { get; set; }
    public string DocumentPath { get; set; } = string.Empty;
    public Guid UploadedById { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
    public virtual AppUser UploadedBy { get; set; } = null!;
}
