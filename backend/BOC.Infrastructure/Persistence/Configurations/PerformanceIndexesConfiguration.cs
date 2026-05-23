using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;
using BOC.Domain.Enums;

namespace BOC.Infrastructure.Persistence.Configurations;

// ─────────────────────────────────────────────────────────────────────────────
// Performance Indexes — applies supplemental covering/composite indexes for
// the highest-frequency query patterns identified in Blueprint Section 13.
// These extend (do not replace) the primary configurations already defined.
//
// Query Patterns Covered:
//   1.  ResearchPapers — list by State + ResearcherId      (dashboard)
//   2.  ResearchPapers — list by State + DepartmentId      (manager view)
//   3.  ResearchPapers — list by CategoryId + State        (specialization filter)
//   4.  EvaluatorAssignments — active by EvaluatorId       (evaluator dashboard)
//   5.  EvaluatorAssignments — pending SLA breach scan     (background job)
//   6.  Notifications — unread by UserId                   (bell icon badge)
//   7.  ChatMessages — by ChannelId + SentAt               (chat pagination)
//   8.  OutboxMessages — pending dispatch                  (OutboxDispatcherJob)
//   9.  EmailLogs — pending delivery                       (EmailDispatcherJob)
//   10. AuditLogs — by EntityName + EntityId               (audit trail search)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Supplemental performance index for ResearchPapers table.
/// Primary configuration: ResearchPaperConfiguration.cs
/// </summary>
public class ResearchPaperIndexConfiguration : IEntityTypeConfiguration<ResearchPaper>
{
    public void Configure(EntityTypeBuilder<ResearchPaper> builder)
    {
        // 1. Dashboard — researcher sees own papers by state
        builder.HasIndex(r => new { r.ResearcherId, r.State })
            .HasDatabaseName("IX_ResearchPapers_ResearcherId_State");

        // 2. Manager view — filter by department and state
        builder.HasIndex(r => new { r.DepartmentId, r.State })
            .HasDatabaseName("IX_ResearchPapers_DepartmentId_State");

        // 3. Specialization filter — category + state
        builder.HasIndex(r => new { r.CategoryId, r.State })
            .HasDatabaseName("IX_ResearchPapers_CategoryId_State");

        // 4. Soft-delete / archive scan — IsArchived flag
        builder.HasIndex(r => r.IsArchived)
            .HasDatabaseName("IX_ResearchPapers_IsArchived");

        // 5. Ministry batch linkage — State = Ministry_Batch_Transit
        builder.HasIndex(r => new { r.State, r.MeetingMinutesId })
            .HasDatabaseName("IX_ResearchPapers_State_MeetingMinutesId");
    }
}

/// <summary>
/// Supplemental performance index for EvaluatorAssignments table.
/// Primary configuration: EvaluatorAssignmentConfiguration.cs
/// </summary>
public class EvaluatorAssignmentIndexConfiguration : IEntityTypeConfiguration<EvaluatorAssignment>
{
    public void Configure(EntityTypeBuilder<EvaluatorAssignment> builder)
    {
        // Active assignments per evaluator — evaluator dashboard
        builder.HasIndex(ea => new { ea.EvaluatorId, ea.Status })
            .HasDatabaseName("IX_EvaluatorAssignments_EvaluatorId_Status");

        // SLA breach scanner — find active, not submitted, past due date
        builder.HasIndex(ea => new { ea.Status, ea.IsSLABreached, ea.DueDate })
            .HasDatabaseName("IX_EvaluatorAssignments_Status_IsSLABreached_DueDate");

        // Research-centric view — all assignments for a paper
        builder.HasIndex(ea => ea.ResearchId)
            .HasDatabaseName("IX_EvaluatorAssignments_ResearchId");
    }
}

/// <summary>
/// Supplemental performance index for Notifications table.
/// Primary configuration: NotificationConfiguration (CoreConfigurations.cs)
/// </summary>
public class NotificationIndexConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Badge count — unread per user
        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("IX_Notifications_UserId_IsRead");

        // Ordered notification list — latest first
        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_UserId_CreatedAt");
    }
}

/// <summary>
/// Supplemental performance index for ChatMessages table.
/// Primary configuration: ChatMessageConfiguration (CoreConfigurations.cs)
/// </summary>
public class ChatMessageIndexConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        // Paginated chat history — latest messages first per channel
        builder.HasIndex(cm => new { cm.ChannelId, cm.SentAt })
            .HasDatabaseName("IX_ChatMessages_ChannelId_SentAt");

        // Unread count per receiver
        builder.HasIndex(cm => new { cm.ReceiverId, cm.IsRead })
            .HasDatabaseName("IX_ChatMessages_ReceiverId_IsRead");
    }
}

/// <summary>
/// Supplemental performance index for OutboxMessages table.
/// Primary configuration: OutboxMessageConfiguration (CoreConfigurations.cs)
/// </summary>
public class OutboxMessageIndexConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        // OutboxDispatcherJob — pick pending messages in order
        builder.HasIndex(om => new { om.ProcessedAt, om.OccurredOn })
            .HasDatabaseName("IX_OutboxMessages_ProcessedAt_OccurredOn");

        // Retry management — find failed messages
        builder.HasIndex(om => new { om.ProcessedAt, om.RetryCount })
            .HasDatabaseName("IX_OutboxMessages_ProcessedAt_RetryCount");
    }
}

/// <summary>
/// Supplemental performance index for EmailLogs table.
/// Primary configuration: EmailLogConfiguration (CoreConfigurations.cs)
/// </summary>
public class EmailLogIndexConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        // EmailDispatcherJob — find pending emails oldest-first
        builder.HasIndex(el => new { el.DeliveryStatus, el.CreatedAt })
            .HasDatabaseName("IX_EmailLogs_DeliveryStatus_CreatedAt");
    }
}

/// <summary>
/// Supplemental performance index for AuditLogs table.
/// Primary configuration: AuditLogConfiguration.cs
/// </summary>
public class AuditLogIndexConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Audit trail query — by table name and record ID
        builder.HasIndex(al => new { al.TableName, al.RecordID })
            .HasDatabaseName("IX_AuditLogs_TableName_RecordID");

        // By actor (EmployeeID) — who changed what, ordered by time
        builder.HasIndex(al => new { al.OperatorEmployeeID, al.Timestamp })
            .HasDatabaseName("IX_AuditLogs_OperatorEmployeeID_Timestamp");
    }
}

/// <summary>
/// Supplemental performance index for PlagiarismLockouts table.
/// Primary configuration: PlagiarismLockoutConfiguration (CoreConfigurations.cs)
/// </summary>
public class PlagiarismLockoutIndexConfiguration : IEntityTypeConfiguration<PlagiarismLockout>
{
    public void Configure(EntityTypeBuilder<PlagiarismLockout> builder)
    {
        // PlagiarismLockoutExpiryJob — find expired lockouts
        builder.HasIndex(pl => pl.LockedUntil)
            .HasDatabaseName("IX_PlagiarismLockouts_LockedUntil");
    }
}

/// <summary>
/// Supplemental performance index for MinistryBatches table.
/// Primary configuration: MinistryBatchConfiguration (CoreConfigurations.cs)
/// </summary>
public class MinistryBatchIndexConfiguration : IEntityTypeConfiguration<MinistryBatch>
{
    public void Configure(EntityTypeBuilder<MinistryBatch> builder)
    {
        // MinistryBatchPollerJob — find pending batches with responses
        builder.HasIndex(mb => new { mb.Status, mb.SentDate })
            .HasDatabaseName("IX_MinistryBatches_Status_SentDate");
    }
}
