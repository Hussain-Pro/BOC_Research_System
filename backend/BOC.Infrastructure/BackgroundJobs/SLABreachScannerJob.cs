using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using BOC.Domain.Entities;
using BOC.Domain.Events;
using BOC.Domain.Enums;
using BOC.Infrastructure.Persistence;

namespace BOC.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class SLABreachScannerJob : IJob
{
    private readonly BOCDbContext _dbContext;
    private readonly ILogger<SLABreachScannerJob> _logger;

    public SLABreachScannerJob(BOCDbContext dbContext, ILogger<SLABreachScannerJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("SLA Breach Scanner job started...");

        var now = DateTime.UtcNow;

        // Scan active assignments that are past due date and not submitted yet
        var overdueAssignments = await _dbContext.EvaluatorAssignments
            .Where(ea => ea.Status == AssignmentStatus.Active 
                      && ea.SubmittedDate == null 
                      && ea.DueDate < now 
                      && !ea.IsSLABreached)
            .ToListAsync(context.CancellationToken);

        if (overdueAssignments.Count == 0)
        {
            _logger.LogInformation("No new SLA breaches found.");
            return;
        }

        _logger.LogWarning("Found {Count} overdue evaluator assignments.", overdueAssignments.Count);

        foreach (var assignment in overdueAssignments)
        {
            assignment.IsSLABreached = true;
            assignment.ModifiedAt = now;
            
            // Add domain event which will be picked up by OutboxInterceptor
            assignment.AddDomainEvent(new SLABreachedEvent(assignment.Id, assignment.ResearchId, assignment.EvaluatorId));
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("SLA Breach Scanner job completed.");
    }
}
