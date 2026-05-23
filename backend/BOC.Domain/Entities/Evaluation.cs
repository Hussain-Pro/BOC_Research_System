using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class Evaluation : Entity
{
    public Guid AssignmentId { get; set; }
    public decimal Score { get; set; }
    public string? Comments { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual EvaluatorAssignment Assignment { get; set; } = null!;
}
