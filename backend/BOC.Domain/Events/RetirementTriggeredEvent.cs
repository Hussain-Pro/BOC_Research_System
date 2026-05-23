using System;

namespace BOC.Domain.Events;

public class RetirementTriggeredEvent
{
    public Guid EvaluatorId { get; }
    public string FullName { get; }
    public DateOnly BirthDate { get; }

    public RetirementTriggeredEvent(Guid evaluatorId, string fullName, DateOnly birthDate)
    {
        EvaluatorId = evaluatorId;
        FullName = fullName;
        BirthDate = birthDate;
    }
}
