using System;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class ChatMessage : Entity
{
    public Guid ChannelId { get; set; }
    public ChannelType ChannelType { get; set; }
    public Guid SenderId { get; set; }
    public Guid? ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsAnonymous { get; set; }
    public bool IsRead { get; set; }
    public Guid? RelatedResearchId { get; set; }

    // Navigation properties
    public virtual ChatChannel Channel { get; set; } = null!;
    public virtual AppUser Sender { get; set; } = null!;
    public virtual AppUser? Receiver { get; set; }
    public virtual ResearchPaper? RelatedResearch { get; set; }
}
