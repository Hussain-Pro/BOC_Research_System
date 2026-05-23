using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class UserSetting : Entity
{
    public Guid UserId { get; set; }
    public string Language { get; set; } = "AR";
    public string Theme { get; set; } = "Light";
    public bool EmailNotificationsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
}
