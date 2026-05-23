using System;
using System.Collections.Generic;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class AppUser : Entity
{
    public string EmployeeID { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NormalizedEmail { get; set; }
    public string? PasswordHash { get; set; }
    public string? SecurityStamp { get; set; }
    public string? ConcurrencyStamp { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid RoleId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DirectorateId { get; set; }
    public DateOnly? BirthDate { get; set; }
    public EvaluatorStatus EvaluatorStatus { get; set; } = EvaluatorStatus.Active;
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Pending_HR_Verification;
    public bool IsEmailConfirmed { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public string? DeviceFingerprint { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual AppRole Role { get; set; } = null!;
    public virtual Department? Department { get; set; }
    public virtual Directorate? Directorate { get; set; }
    public virtual UserSetting? UserSetting { get; set; }

    public virtual ICollection<EvaluatorSpecialization> EvaluatorSpecializations { get; set; } = new List<EvaluatorSpecialization>();
    public virtual ICollection<DelegatedRole> DelegatedFromRoles { get; set; } = new List<DelegatedRole>();
    public virtual ICollection<DelegatedRole> DelegatedToRoles { get; set; } = new List<DelegatedRole>();
    public virtual ICollection<ResearchPaper> Submissions { get; set; } = new List<ResearchPaper>();
    public virtual ICollection<EvaluatorAssignment> Assignments { get; set; } = new List<EvaluatorAssignment>();
    public virtual ICollection<Meeting> CreatedMeetings { get; set; } = new List<Meeting>();
}
