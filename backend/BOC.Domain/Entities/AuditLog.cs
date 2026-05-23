using System;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class AuditLog : Entity
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string OperatorEmployeeID { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string RecordID { get; set; } = string.Empty;
    public string? OldValueJSON { get; set; }
    public string? NewValueJSON { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
