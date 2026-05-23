using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using BOC.Domain.Common;
using BOC.Domain.Entities;

namespace BOC.Infrastructure.Persistence.Interceptors;

public class OutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var outboxMessages = CreateOutboxMessages(context);
        if (outboxMessages.Any())
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChanges(eventData, result);

        var outboxMessages = CreateOutboxMessages(context);
        if (outboxMessages.Any())
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }

        return base.SavingChanges(eventData, result);
    }

    private List<OutboxMessage> CreateOutboxMessages(DbContext context)
    {
        var entitiesWithEvents = context.ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();

        var outboxMessages = new List<OutboxMessage>();

        foreach (var entry in entitiesWithEvents)
        {
            var entity = entry.Entity;
            foreach (var domainEvent in entity.DomainEvents)
            {
                var eventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().Name;
                var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);

                var outboxMessage = new OutboxMessage
                {
                    EventType = eventType,
                    Payload = payload,
                    OccurredOn = DateTime.UtcNow,
                    ProcessedAt = null,
                    Error = null,
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                outboxMessages.Add(outboxMessage);
            }

            entity.ClearDomainEvents();
        }

        return outboxMessages;
    }
}
