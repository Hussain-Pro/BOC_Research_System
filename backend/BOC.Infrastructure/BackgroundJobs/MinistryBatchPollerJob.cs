using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Enums;
using BOC.Infrastructure.Persistence;

namespace BOC.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs every 12 hours.
/// Polls MinistryBatches for any Pending batches that may have received a
/// MinistryResponseJson (delivered externally via SFTP / API gateway).
/// When a response is found:
///   - Parse MinistryDecision per BatchItem (Approved / Rejected).
///   - Transition linked ResearchPaper from Ministry_Batch_Transit
///     → Pass_Approved  (if Approved)
///     → Fail_Rejected  (if Rejected)
///   - Emit ResearchStateChangedEvent via domain event / Outbox.
///   - Mark MinistryBatch.Status as Approved or Rejected.
///
/// In this implementation the job scans for batches where MinistryResponseJson
/// has been populated but Status is still Pending (meaning the response file
/// arrived via FTP/SFTP and was placed in the DB by an operator or webhook).
///
/// Blueprint Reference: Section 8 — MinistryBatchPollerJob, Every 12 hours.
///                      Section 5.1 — Ministry_Batch_Transit transitions.
/// </summary>
[DisallowConcurrentExecution]
public sealed class MinistryBatchPollerJob : IJob
{
    private readonly BOCDbContext _dbContext;
    private readonly ILogger<MinistryBatchPollerJob> _logger;

    public MinistryBatchPollerJob(
        BOCDbContext dbContext,
        ILogger<MinistryBatchPollerJob> logger)
    {
        _dbContext = dbContext;
        _logger    = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[MinistryBatchPollerJob] Polling ministry batches for responses...");

        var cancellationToken = context.CancellationToken;
        var now               = DateTime.UtcNow;

        // Find pending batches that have received a MinistryResponseJson
        var pendingBatches = await _dbContext.MinistryBatches
            .Include(b => b.Items)
                .ThenInclude(bi => bi.ResearchPaper)
            .Where(b =>
                b.Status == MinistryBatchStatus.Pending &&
                b.MinistryResponseJson != null)
            .ToListAsync(cancellationToken);

        if (pendingBatches.Count == 0)
        {
            _logger.LogInformation("[MinistryBatchPollerJob] No pending ministry batch responses found.");
            return;
        }

        _logger.LogInformation(
            "[MinistryBatchPollerJob] Found {Count} batch(es) with ministry responses to process.",
            pendingBatches.Count);

        foreach (var batch in pendingBatches)
        {
            _logger.LogInformation(
                "[MinistryBatchPollerJob] Processing batch #{BatchNumber} (Id: {Id}).",
                batch.BatchNumber, batch.Id);

            var allApproved = true;

            foreach (var item in batch.Items)
            {
                if (item.ResearchPaper == null ||
                    item.ResearchPaper.State != ResearchState.Ministry_Batch_Transit)
                {
                    _logger.LogWarning(
                        "[MinistryBatchPollerJob] BatchItem {Id}: Research not in Ministry_Batch_Transit state — skipping.",
                        item.Id);
                    continue;
                }

                var paper = item.ResearchPaper;

                // Parse the individual decision per item from MinistryDecision column
                if (string.IsNullOrWhiteSpace(item.MinistryDecision))
                {
                    _logger.LogWarning(
                        "[MinistryBatchPollerJob] BatchItem {Id}: No MinistryDecision populated — skipping.",
                        item.Id);
                    continue;
                }

                var isApproved = item.MinistryDecision.Equals("Approved", StringComparison.OrdinalIgnoreCase);
                var previousState = paper.State;

                if (isApproved)
                {
                    paper.State      = ResearchState.Pass_Approved;
                    paper.ModifiedAt = now;
                }
                else
                {
                    paper.State      = ResearchState.Fail_Rejected;
                    paper.ModifiedAt = now;
                    allApproved      = false;
                }

                // Emit domain event for state change — captured atomically by OutboxInterceptor
                paper.AddDomainEvent(new Domain.Events.ResearchStateChangedEvent(
                    researchId:    paper.Id,
                    previousState: previousState,
                    newState:      paper.State,
                    triggeredBy:   "System:MinistryBatchPollerJob"));

                _logger.LogInformation(
                    "[MinistryBatchPollerJob] Research {Id} (#{Tracking}) → {NewState} (Ministry decision: {Decision}).",
                    paper.Id, paper.TrackingNumber, paper.State, item.MinistryDecision);
            }

            // Update batch status based on outcome of all items
            batch.Status     = allApproved ? MinistryBatchStatus.Approved : MinistryBatchStatus.Rejected;
            batch.ModifiedAt = now;

            _logger.LogInformation(
                "[MinistryBatchPollerJob] Batch #{BatchNumber} marked as {Status}.",
                batch.BatchNumber, batch.Status);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[MinistryBatchPollerJob] Ministry batch polling completed.");
    }
}
