using System;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class EmailLog : Entity
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? TemplateType { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public EmailDeliveryStatus DeliveryStatus { get; set; } = EmailDeliveryStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
