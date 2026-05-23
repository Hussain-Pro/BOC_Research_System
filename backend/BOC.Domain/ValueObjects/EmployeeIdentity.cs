using System;

namespace BOC.Domain.ValueObjects;

public class EmployeeIdentity : IEquatable<EmployeeIdentity>
{
    public string EmployeeID { get; }
    public string NationalID { get; }

    public EmployeeIdentity(string employeeId, string nationalId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("EmployeeID cannot be empty.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(nationalId))
            throw new ArgumentException("NationalID cannot be empty.", nameof(nationalId));

        EmployeeID = employeeId.Trim();
        NationalID = nationalId.Trim();
    }

    public bool Equals(EmployeeIdentity? other)
    {
        if (other is null) return false;
        return string.Equals(EmployeeID, other.EmployeeID, StringComparison.OrdinalIgnoreCase)
            && string.Equals(NationalID, other.NationalID, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as EmployeeIdentity);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EmployeeID.ToLowerInvariant(), NationalID.ToLowerInvariant());
    }

    public static bool operator ==(EmployeeIdentity? left, EmployeeIdentity? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(EmployeeIdentity? left, EmployeeIdentity? right)
    {
        return !(left == right);
    }
}
