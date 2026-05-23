using System;

namespace BOC.Domain.Events;

public class ResearchSubmittedEvent
{
    public Guid ResearchId { get; }
    public string TrackingNumber { get; }
    public string Title { get; }
    public Guid ResearcherId { get; }

    public ResearchSubmittedEvent(Guid researchId, string trackingNumber, string title, Guid researcherId)
    {
        ResearchId = researchId;
        TrackingNumber = trackingNumber;
        Title = title;
        ResearcherId = researcherId;
    }
}
