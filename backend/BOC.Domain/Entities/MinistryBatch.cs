using System;
using System.Collections.Generic;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class MinistryBatch : Entity
{
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime SentDate { get; set; } = DateTime.UtcNow;
    public string? MinistryResponseJson { get; set; }
    public MinistryBatchStatus Status { get; set; } = MinistryBatchStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual ICollection<BatchItem> Items { get; set; } = new List<BatchItem>();
}
