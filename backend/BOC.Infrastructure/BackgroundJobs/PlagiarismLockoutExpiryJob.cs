using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using BOC.Domain.Enums;
using BOC.Infrastructure.Persistence;

namespace BOC.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs daily at 03:00 UTC.
/// Scans PlagiarismLockouts where LockedUntil &lt; NOW and the associated
/// ResearchPaper has not yet been transitioned to Fail_Rejected.
/// Auto-transitions the paper to Fail_Rejected and records an audit trail
/// via the OutboxInterceptor (ResearchStateChangedEvent).
/// Blueprint Reference: Section 8 — PlagiarismLockoutExpiryJob, Daily 03:00 UTC.
/// </summary>
[DisallowConcurrentExecution]
public sealed class PlagiarismLockoutExpiryJob : IJob
{
    private readonly BOCDbContext _dbContext;
    private readonly ILogger<PlagiarismLockoutExpiryJob> _logger;

    public PlagiarismLockoutExpiryJob(BOCDbContext dbContext, ILogger<PlagiarismLockoutExpiryJob> logger)
    {
        _dbContext = dbContext;
        _logger    = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[PlagiarismLockoutExpiryJob] Starting plagiarism lockout expiry scan...");

        var now = DateTime.UtcNow;
        var cancellationToken = context.CancellationToken;

        // Find all expired plagiarism lockouts whose research is still in Suspended state
        var expiredLockouts = await _dbContext.PlagiarismLockouts
            .Include(pl => pl.ResearchPaper)
            .Where(pl =>
                pl.LockedUntil < now &&
                pl.ResearchPaper != null &&
                pl.ResearchPaper.State == ResearchState.Suspended_Plagiarism_Lockout)
            .ToListAsync(cancellationToken);

        if (expiredLockouts.Count == 0)
        {
            _logger.LogInformation("[PlagiarismLockoutExpiryJob] No expired plagiarism lockouts found.");
            return;
        }

        _logger.LogWarning(
            "[PlagiarismLockoutExpiryJob] Found {Count} expired plagiarism lockout(s) — auto-transitioning to Fail_Rejected.",
            expiredLockouts.Count);

        foreach (var lockout in expiredLockouts)
        {
            var paper = lockout.ResearchPaper!;
            var previousState = paper.State;

            // Auto-transition: Suspended_Plagiarism_Lockout → Fail_Rejected
            paper.State      = ResearchState.Fail_Rejected;
            paper.ModifiedAt = now;

            // Emit domain event — OutboxInterceptor will capture it atomically
            paper.AddDomainEvent(new Domain.Events.ResearchStateChangedEvent(
                researchId:    paper.Id,
                previousState: previousState,
                newState:      ResearchState.Fail_Rejected,
                triggeredBy:   "System:PlagiarismLockoutExpiryJob"));

            _logger.LogInformation(
                "[PlagiarismLockoutExpiryJob] Research {Id} (#{Tracking}) → Fail_Rejected after plagiarism lockout expiry.",
                paper.Id, paper.TrackingNumber);
        }

        var saved = await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "[PlagiarismLockoutExpiryJob] Completed. {Saved} record(s) updated.",
            saved);
    }
}
