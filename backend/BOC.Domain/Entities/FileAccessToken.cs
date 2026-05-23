using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class FileAccessToken : Entity
{
    public Guid AttachmentId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ResearchAttachment Attachment { get; set; } = null!;
    public virtual AppUser CreatedBy { get; set; } = null!;
}
