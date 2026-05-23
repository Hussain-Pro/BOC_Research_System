using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class EvaluatorSpecialization : Entity
{
    public Guid EvaluatorId { get; set; }
    public Guid SpecializationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual AppUser Evaluator { get; set; } = null!;
    public virtual Specialization Specialization { get; set; } = null!;
}
