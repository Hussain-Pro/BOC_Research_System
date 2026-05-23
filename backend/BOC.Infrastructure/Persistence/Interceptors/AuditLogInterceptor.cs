using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using BOC.Domain.Entities;
using BOC.Domain.Enums;

namespace BOC.Infrastructure.Persistence.Interceptors;

public class AuditLogInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = CreateAuditLogs(context);
        if (auditEntries.Any())
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChanges(eventData, result);

        var auditEntries = CreateAuditLogs(context);
        if (auditEntries.Any())
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }

        return base.SavingChanges(eventData, result);
    }

    private List<AuditLog> CreateAuditLogs(DbContext context)
    {
        context.ChangeTracker.DetectChanges();
        var auditLogs = new List<AuditLog>();

        // Find current operator ID (e.g. from an HTTP Context accessor or hardcoded/system default if running in a background job)
        // For simplicity, we can default to "SYSTEM" or pass a service that retrieves it.
        var operatorId = "SYSTEM"; // In actual WebAPI we'll configure it via current user service

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.Entity is OutboxMessage || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            {
                continue;
            }

            var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.Name;
            var recordId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "Unknown";

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var actionType = entry.State.ToString();

            if (entry.State == EntityState.Added)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey()) continue;
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                foreach (var prop in entry.Properties)
                {
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified)
                    {
                        oldValues[prop.Metadata.Name] = prop.OriginalValue;
                        newValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
            }

            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                OperatorEmployeeID = operatorId,
                ActionType = actionType,
                TableName = tableName,
                RecordID = recordId,
                OldValueJSON = oldValues.Any() ? JsonSerializer.Serialize(oldValues, JsonOptions) : null,
                NewValueJSON = newValues.Any() ? JsonSerializer.Serialize(newValues, JsonOptions) : null,
                IpAddress = "127.0.0.1", // Standard fallback
                UserAgent = "BOC System Core"
            };

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }
}
