using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

/// <summary>
/// Defines the scope boundary for a specific permission.
/// Allows fine-grained control: e.g., a Supervisor can read research
/// only within their own Directorate (ScopeType = "Directorate", ScopeValue = directorateId).
/// Blueprint Reference: Section 2.3 — PermissionScopes table.
/// </summary>
public class PermissionScope : Entity
{
    /// <summary>FK to the permission being scoped.</summary>
    public Guid PermissionId { get; set; }

    /// <summary>The dimension being scoped (e.g., "Directorate", "Department", "Role").</summary>
    public string ScopeType { get; set; } = string.Empty;

    /// <summary>The actual value for the scope (e.g., the Guid of the directorate).</summary>
    public string ScopeValue { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
