using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BOC.Domain.Entities;

namespace BOC.Application.Common.Interfaces;

public interface IBOCDbContext
{
    DbSet<AppUser> AppUsers { get; }
    DbSet<AppRole> AppRoles { get; }
    DbSet<Directorate> Directorates { get; }
    DbSet<Department> Departments { get; }
    DbSet<Specialization> Specializations { get; }
    DbSet<EvaluatorSpecialization> EvaluatorSpecializations { get; }
    DbSet<UserSetting> UserSettings { get; }
    DbSet<DelegatedRole> DelegatedRoles { get; }
    
    DbSet<ResearchCategory> ResearchCategories { get; }
    DbSet<ResearchPaper> ResearchPapers { get; }
    DbSet<ResearchVersion> ResearchVersions { get; }
    DbSet<ResearchCorrection> ResearchCorrections { get; }
    DbSet<Substitution> Substitutions { get; }
    DbSet<PlagiarismLockout> PlagiarismLockouts { get; }
    DbSet<PlagiarismOverrideJustification> PlagiarismOverrideJustifications { get; }
    
    DbSet<Meeting> Meetings { get; }
    DbSet<MeetingAgenda> MeetingAgendas { get; }
    DbSet<MeetingMinutes> MeetingMinutes { get; }
    DbSet<FreezeEvent> FreezeEvents { get; }
    DbSet<MeetingRSVP> MeetingRSVPs { get; }
    DbSet<MeetingAttendance> MeetingAttendances { get; }
    DbSet<Vote> Votes { get; }
    
    DbSet<EvaluatorAssignment> EvaluatorAssignments { get; }
    DbSet<Evaluation> Evaluations { get; }
    DbSet<ChairmanScore> ChairmanScores { get; }
    DbSet<TriageMapping> TriageMappings { get; }
    
    DbSet<ChatChannel> ChatChannels { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<Notification> Notifications { get; }
    
    DbSet<MinistryBatch> MinistryBatches { get; }
    DbSet<BatchItem> BatchItems { get; }
    
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<HRVerificationQueue> HRVerificationQueue { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<EmailLog> EmailLogs { get; }
    DbSet<SystemConfiguration> SystemConfigurations { get; }
    DbSet<SystemConfigurationHistory> SystemConfigurationHistory { get; }
    DbSet<ResearchAttachment> ResearchAttachments { get; }
    DbSet<FileAccessToken> FileAccessTokens { get; }
    DbSet<PermissionScope> PermissionScopes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
