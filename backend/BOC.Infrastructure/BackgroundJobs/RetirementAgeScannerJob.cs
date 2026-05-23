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
public class RetirementAgeScannerJob : IJob
{
    private readonly BOCDbContext _dbContext;
    private readonly ILogger<RetirementAgeScannerJob> _logger;

    public RetirementAgeScannerJob(BOCDbContext dbContext, ILogger<RetirementAgeScannerJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Retirement Age Scanner job started...");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Fetch active evaluators / committee members
        var activeEvaluators = await _dbContext.AppUsers
            .Include(u => u.Role)
            .Where(u => (u.Role.Name == "External Evaluators" || u.Role.Name == "Committee Members")
                     && u.EvaluatorStatus == EvaluatorStatus.Active
                     && u.BirthDate != null)
            .ToListAsync(context.CancellationToken);

        int lockedCount = 0;

        foreach (var user in activeEvaluators)
        {
            var birthDate = user.BirthDate!.Value;
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) 
            {
                age--;
            }

            if (age >= 63)
            {
                _logger.LogWarning("Evaluator {Name} (ID: {Id}) reached retirement age of 63 (Actual Age: {Age}). Suspending temporarily...", user.FullName, user.Id, age);
                
                user.EvaluatorStatus = EvaluatorStatus.Suspended_Temporarily;
                user.ModifiedAt = DateTime.UtcNow;
                
                user.AddDomainEvent(new RetirementTriggeredEvent(user.Id, user.FullName, birthDate));
                lockedCount++;
            }
        }

        if (lockedCount > 0)
        {
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Suspended {Count} retired evaluators.", lockedCount);
        }
        else
        {
            _logger.LogInformation("No new retirement suspensions needed.");
        }

        _logger.LogInformation("Retirement Age Scanner job completed.");
    }
}
