using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using BOC.Domain.Entities;
using BOC.Infrastructure.Persistence;

namespace BOC.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class OutboxDispatcherJob : IJob
{
    private readonly BOCDbContext _dbContext;
    private readonly IPublisher _publisher;
    private readonly ILogger<OutboxDispatcherJob> _logger;

    public OutboxDispatcherJob(
        BOCDbContext dbContext,
        IPublisher publisher,
        ILogger<OutboxDispatcherJob> logger)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Outbox Dispatcher started processing...");

        var messages = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < 5)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    _logger.LogError("Could not load type {EventType}", message.EventType);
                    message.Error = "Type not found";
                    message.RetryCount++;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType);
                if (domainEvent == null)
                {
                    _logger.LogError("Failed to deserialize event {Id}", message.Id);
                    message.Error = "Deserialization failed";
                    message.RetryCount++;
                    continue;
                }

                await _publisher.Publish(domainEvent, context.CancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {Id}", message.Id);
                message.Error = ex.ToString();
                message.RetryCount++;
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Outbox Dispatcher completed processing.");
    }
}
