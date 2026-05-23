using System;

namespace BOC.Domain.Events;

public class EvaluatorAssignedEvent
{
    public Guid AssignmentId { get; }
    public Guid ResearchId { get; }
    public Guid EvaluatorId { get; }
    public DateTime DueDate { get; }

    public EvaluatorAssignedEvent(Guid assignmentId, Guid researchId, Guid evaluatorId, DateTime dueDate)
    {
        AssignmentId = assignmentId;
        ResearchId = researchId;
        EvaluatorId = evaluatorId;
        DueDate = dueDate;
    }
}
