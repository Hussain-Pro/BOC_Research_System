using System;
using System.Text.RegularExpressions;

namespace BOC.Domain.ValueObjects;

public class FileHash : IEquatable<FileHash>
{
    private static readonly Regex Sha256Regex = new("^[a-fA-F0-9]{64}$", RegexOptions.Compiled);

    public string Value { get; }

    public FileHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Hash value cannot be empty.", nameof(value));

        var trimmed = value.Trim();
        if (!Sha256Regex.IsMatch(trimmed))
            throw new ArgumentException("Invalid SHA-256 hash format. Must be 64 hexadecimal characters.", nameof(value));

        Value = trimmed.ToLowerInvariant();
    }

    public bool Equals(FileHash? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as FileHash);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(FileHash? left, FileHash? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(FileHash? left, FileHash? right)
    {
        return !(left == right);
    }

    public override string ToString() => Value;
}
