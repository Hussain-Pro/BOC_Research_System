using System;

namespace BOC.Domain.Events;

public class SLABreachedEvent
{
    public Guid AssignmentId { get; }
    public Guid ResearchId { get; }
    public Guid EvaluatorId { get; }

    public SLABreachedEvent(Guid assignmentId, Guid researchId, Guid evaluatorId)
    {
        AssignmentId = assignmentId;
        ResearchId = researchId;
        EvaluatorId = evaluatorId;
    }
}
