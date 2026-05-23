using System;
using BOC.Domain.Enums;

namespace BOC.Domain.Events;

/// <summary>
/// Emitted whenever a ResearchPaper transitions between states.
/// Captured by the OutboxInterceptor and dispatched to:
///   - SignalRHub (live badge update)
///   - AuditLogHandler (audit trail)
///   - SLAHandler (SLA recalculation)
///   - NotificationHandler (user notifications)
/// Blueprint Reference: Section 7 — ResearchStateChanged domain event.
/// </summary>
public sealed class ResearchStateChangedEvent
{
    public Guid          ResearchId     { get; }
    public ResearchState PreviousState  { get; }
    public ResearchState NewState       { get; }
    public string        TriggeredBy    { get; }
    public DateTime      OccurredAt     { get; }

    public ResearchStateChangedEvent(
        Guid          researchId,
        ResearchState previousState,
        ResearchState newState,
        string        triggeredBy)
    {
        ResearchId    = researchId;
        PreviousState = previousState;
        NewState      = newState;
        TriggeredBy   = triggeredBy;
        OccurredAt    = DateTime.UtcNow;
    }
}
