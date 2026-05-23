using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class SystemConfigurationHistory : Entity
{
    public string ConfigKey { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string NewValue { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual AppUser? ModifiedBy { get; set; }
}
