using System;
using System.Collections.Generic;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class EvaluatorAssignment : Entity
{
    public Guid ResearchId { get; set; }
    public Guid EvaluatorId { get; set; }
    public Guid AssignedById { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public int ReminderCount { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Active;
    public bool IsSLABreached { get; set; } = false;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual ResearchPaper ResearchPaper { get; set; } = null!;
    public virtual AppUser Evaluator { get; set; } = null!;
    public virtual AppUser AssignedBy { get; set; } = null!;

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}
