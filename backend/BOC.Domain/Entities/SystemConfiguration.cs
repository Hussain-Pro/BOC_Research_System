using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class SystemConfiguration : Entity
{
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedById { get; set; }

    // Navigation properties
    public virtual AppUser? ModifiedBy { get; set; }
}
