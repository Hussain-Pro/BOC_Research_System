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
/// Runs every 5 minutes (event-triggered behavior simulated via frequent polling).
/// Processes pending email notifications sourced from EmailLogs records
/// that were created by domain event handlers but not yet dispatched.
///
/// Responsibilities:
///   1. Scan EmailLogs where DeliveryStatus == Pending.
///   2. Attempt SMTP delivery via IEmailSender.
///   3. Mark DeliveryStatus = Sent or Failed accordingly.
///   4. Also process SLA 10-day warning emails for evaluator assignments
///      approaching their due date (DueDate within next 4 days + ReminderCount &lt; 3).
///
/// Blueprint Reference: Section 8 — EmailDispatcherJob, triggered by event.
///                      Section 6, Rule 7 — SLA reminder emails at 10-day threshold.
///                      Section 11 — Hangfire-triggered reminder emails for SLA.
/// </summary>
[DisallowConcurrentExecution]
public sealed class EmailDispatcherJob : IJob
{
    private readonly BOCDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailDispatcherJob> _logger;

    private const int MaxRetryAttemptsPerRun = 10;

    public EmailDispatcherJob(
        BOCDbContext dbContext,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<EmailDispatcherJob> logger)
    {
        _dbContext     = dbContext;
        _emailSender   = emailSender;
        _configuration = configuration;
        _logger        = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[EmailDispatcherJob] Starting email dispatch run...");

        var cancellationToken = context.CancellationToken;
        var now               = DateTime.UtcNow;
        var totalSent         = 0;
        var totalFailed       = 0;

        // ── 1. Dispatch pending EmailLog entries ──────────────────────────────
        var pendingEmails = await _dbContext.EmailLogs
            .Where(e => e.DeliveryStatus == EmailDeliveryStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .Take(MaxRetryAttemptsPerRun)
            .ToListAsync(cancellationToken);

        foreach (var emailLog in pendingEmails)
        {
            try
            {
                await _emailSender.SendEmailAsync(
                    emailLog.RecipientEmail,
                    emailLog.Subject,
                    BuildEmailBody(emailLog.TemplateType, emailLog.Subject),
                    cancellationToken);

                emailLog.DeliveryStatus = EmailDeliveryStatus.Sent;
                emailLog.SentAt         = now;
                totalSent++;

                _logger.LogInformation(
                    "[EmailDispatcherJob] Email sent to {Recipient} (type: {Type}).",
                    emailLog.RecipientEmail, emailLog.TemplateType);
            }
            catch (Exception ex)
            {
                emailLog.DeliveryStatus = EmailDeliveryStatus.Failed;
                emailLog.ErrorMessage   = ex.Message;
                totalFailed++;

                _logger.LogError(
                    ex,
                    "[EmailDispatcherJob] Failed to send email to {Recipient} (type: {Type}).",
                    emailLog.RecipientEmail, emailLog.TemplateType);
            }
        }

        // ── 2. SLA 10-day warning emails ──────────────────────────────────────
        // Evaluators whose DueDate is within next 4 days and ReminderCount < 3
        var slaWarningThresholdDays = _configuration.GetValue<int>("Sla:WarningThresholdDays", 10);
        var warningWindowStart      = now;
        var warningWindowEnd        = now.AddDays(slaWarningThresholdDays - 6); // 4-day window before 10-day mark

        var assignmentsNeedingReminder = await _dbContext.EvaluatorAssignments
            .Include(ea => ea.Evaluator)
            .Include(ea => ea.ResearchPaper)
            .Where(ea =>
                ea.Status == AssignmentStatus.Active &&
                ea.SubmittedDate == null &&
                !ea.IsSLABreached &&
                ea.ReminderCount < 3 &&
                ea.DueDate >= warningWindowStart &&
                ea.DueDate <= warningWindowEnd)
            .ToListAsync(cancellationToken);

        foreach (var assignment in assignmentsNeedingReminder)
        {
            if (assignment.Evaluator?.Email == null) continue;

            var daysRemaining = (int)(assignment.DueDate - now).TotalDays;
            var subject = $"تذكير: موعد تقييم البحث يقترب ({daysRemaining} يوم متبقي)";
            var body    = BuildSlaReminderBody(
                assignment.Evaluator.FullName,
                assignment.ResearchPaper?.TrackingNumber ?? "N/A",
                assignment.ResearchPaper?.Title ?? "N/A",
                daysRemaining,
                assignment.DueDate);

            try
            {
                await _emailSender.SendEmailAsync(
                    assignment.Evaluator.Email,
                    subject,
                    body,
                    cancellationToken);

                assignment.ReminderCount++;
                assignment.ModifiedAt = now;
                totalSent++;

                _logger.LogInformation(
                    "[EmailDispatcherJob] SLA reminder #{Count} sent to evaluator {Email} for research #{Tracking}.",
                    assignment.ReminderCount, assignment.Evaluator.Email, assignment.ResearchPaper?.TrackingNumber);
            }
            catch (Exception ex)
            {
                totalFailed++;
                _logger.LogError(
                    ex,
                    "[EmailDispatcherJob] Failed to send SLA reminder to {Email}.",
                    assignment.Evaluator.Email);
            }
        }

        // ── Save all changes ──────────────────────────────────────────────────
        if (pendingEmails.Count > 0 || assignmentsNeedingReminder.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "[EmailDispatcherJob] Completed. Sent: {Sent}, Failed: {Failed}.",
            totalSent, totalFailed);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static string BuildEmailBody(string templateType, string subject)
    {
        // Generic fallback body — in production this would load from an HTML template
        return $@"
<html>
<body dir='rtl' style='font-family: Tajawal, Arial, sans-serif; color: #0F2A38;'>
  <div style='max-width:600px; margin:0 auto; padding:24px; border:1px solid #e0e0e0; border-radius:6px;'>
    <h2 style='color:#163E54;'>نظام تقييم البحوث — بصرة أويل</h2>
    <p>{subject}</p>
    <hr style='border-color:#163E54;'/>
    <p style='font-size:12px; color:#4A607A;'>
      هذه رسالة آلية من نظام إدارة وتقييم البحوث — شركة نفط البصرة.
    </p>
  </div>
</body>
</html>";
    }

    private static string BuildSlaReminderBody(
        string evaluatorName,
        string trackingNumber,
        string researchTitle,
        int daysRemaining,
        DateTime dueDate)
    {
        return $@"
<html>
<body dir='rtl' style='font-family: Tajawal, Arial, sans-serif; color: #0F2A38;'>
  <div style='max-width:600px; margin:0 auto; padding:24px; border:1px solid #e0e0e0; border-radius:6px;'>
    <h2 style='color:#163E54;'>تذكير بموعد تقييم البحث</h2>
    <p>السيد/ة <strong>{evaluatorName}</strong>،</p>
    <p>نذكّركم بأن الموعد النهائي لتقييم البحث المُسند إليكم يقترب:</p>
    <table style='width:100%; border-collapse:collapse; margin:16px 0;'>
      <tr>
        <td style='padding:8px; background:#F4F6F9; border:1px solid #ddd; font-weight:bold;'>رقم التتبع:</td>
        <td style='padding:8px; border:1px solid #ddd;'>{trackingNumber}</td>
      </tr>
      <tr>
        <td style='padding:8px; background:#F4F6F9; border:1px solid #ddd; font-weight:bold;'>عنوان البحث:</td>
        <td style='padding:8px; border:1px solid #ddd;'>{researchTitle}</td>
      </tr>
      <tr>
        <td style='padding:8px; background:#F4F6F9; border:1px solid #ddd; font-weight:bold;'>الموعد النهائي:</td>
        <td style='padding:8px; border:1px solid #ddd;'>{dueDate:yyyy-MM-dd}</td>
      </tr>
      <tr>
        <td style='padding:8px; background:#fff3cd; border:1px solid #ddd; font-weight:bold; color:#d9534f;'>الأيام المتبقية:</td>
        <td style='padding:8px; background:#fff3cd; border:1px solid #ddd; font-weight:bold; color:#d9534f;'>{daysRemaining} يوم</td>
      </tr>
    </table>
    <p>يرجى إتمام التقييم في أقرب وقت ممكن لتجنب تجاوز المدة المحددة.</p>
    <hr style='border-color:#163E54;'/>
    <p style='font-size:12px; color:#4A607A;'>
      هذه رسالة آلية من نظام إدارة وتقييم البحوث — شركة نفط البصرة.
    </p>
  </div>
</body>
</html>";
    }
}
