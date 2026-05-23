using System;
using System.Collections.Generic;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class Meeting : Entity
{
    public string MeetingNumber { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? Location { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual AppUser CreatedBy { get; set; } = null!;
    public virtual ICollection<MeetingAgenda> Agendas { get; set; } = new List<MeetingAgenda>();
    public virtual ICollection<MeetingMinutes> Minutes { get; set; } = new List<MeetingMinutes>();
    public virtual ICollection<MeetingRSVP> RSVPs { get; set; } = new List<MeetingRSVP>();
    public virtual ICollection<MeetingAttendance> Attendances { get; set; } = new List<MeetingAttendance>();
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public virtual ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
}
