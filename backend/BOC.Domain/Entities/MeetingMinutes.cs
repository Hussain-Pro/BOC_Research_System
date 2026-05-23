using System;
using System.Collections.Generic;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class MeetingMinutes : Entity
{
    public Guid MeetingId { get; set; }
    public string MinutesNumber { get; set; } = string.Empty;
    public string? Content { get; set; }
    public MeetingMinutesStatus Status { get; set; } = MeetingMinutesStatus.Draft;
    public DateTime? SignedDate { get; set; }
    public Guid? SignedById { get; set; }
    public bool IsFrozen { get; private set; } // Database computed column
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
    public virtual AppUser? SignedBy { get; set; }
    public virtual ICollection<FreezeEvent> FreezeEvents { get; set; } = new List<FreezeEvent>();
    public virtual ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
    public virtual ICollection<ChairmanScore> ChairmanScores { get; set; } = new List<ChairmanScore>();
}
