using System;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class HRVerificationQueue : Entity
{
    public Guid UserId { get; set; }
    public string EmployeeID { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedById { get; set; }
    public HRVerificationStatus Status { get; set; } = HRVerificationStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual AppUser? VerifiedBy { get; set; }
}
