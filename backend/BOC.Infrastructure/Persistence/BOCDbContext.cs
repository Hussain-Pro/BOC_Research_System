using Microsoft.EntityFrameworkCore;
using BOC.Domain.Entities;
using BOC.Application.Common.Interfaces;

namespace BOC.Infrastructure.Persistence;

public class BOCDbContext : DbContext, IBOCDbContext
{
    public BOCDbContext(DbContextOptions<BOCDbContext> options) : base(options)
    {
    }

    // ── Identity & Directory ─────────────────────────────────────────────────
    public DbSet<AppUser>                    AppUsers                   => Set<AppUser>();
    public DbSet<AppRole>                    AppRoles                   => Set<AppRole>();
    public DbSet<Directorate>                Directorates               => Set<Directorate>();
    public DbSet<Department>                 Departments                => Set<Department>();
    public DbSet<Specialization>             Specializations            => Set<Specialization>();
    public DbSet<EvaluatorSpecialization>    EvaluatorSpecializations   => Set<EvaluatorSpecialization>();
    public DbSet<UserSetting>                UserSettings               => Set<UserSetting>();
    public DbSet<DelegatedRole>              DelegatedRoles             => Set<DelegatedRole>();
    public DbSet<PermissionScope>            PermissionScopes           => Set<PermissionScope>();

    // ── Research Lifecycle ───────────────────────────────────────────────────
    public DbSet<ResearchCategory>              ResearchCategories              => Set<ResearchCategory>();
    public DbSet<ResearchPaper>                 ResearchPapers                  => Set<ResearchPaper>();
    public DbSet<ResearchVersion>               ResearchVersions                => Set<ResearchVersion>();
    public DbSet<ResearchCorrection>            ResearchCorrections             => Set<ResearchCorrection>();
    public DbSet<Substitution>                  Substitutions                   => Set<Substitution>();
    public DbSet<PlagiarismLockout>             PlagiarismLockouts              => Set<PlagiarismLockout>();
    public DbSet<PlagiarismOverrideJustification> PlagiarismOverrideJustifications => Set<PlagiarismOverrideJustification>();

    // ── Meetings & Governance ────────────────────────────────────────────────
    public DbSet<Meeting>             Meetings             => Set<Meeting>();
    public DbSet<MeetingAgenda>       MeetingAgendas       => Set<MeetingAgenda>();
    public DbSet<MeetingMinutes>      MeetingMinutes       => Set<MeetingMinutes>();
    public DbSet<FreezeEvent>         FreezeEvents         => Set<FreezeEvent>();
    public DbSet<MeetingRSVP>         MeetingRSVPs         => Set<MeetingRSVP>();
    public DbSet<MeetingAttendance>   MeetingAttendances   => Set<MeetingAttendance>();
    public DbSet<Vote>                Votes                => Set<Vote>();

    // ── Triage & Evaluation ──────────────────────────────────────────────────
    public DbSet<EvaluatorAssignment> EvaluatorAssignments => Set<EvaluatorAssignment>();
    public DbSet<Evaluation>          Evaluations          => Set<Evaluation>();
    public DbSet<ChairmanScore>       ChairmanScores       => Set<ChairmanScore>();
    public DbSet<TriageMapping>       TriageMappings       => Set<TriageMapping>();

    // ── Collaboration & Notifications ────────────────────────────────────────
    public DbSet<ChatChannel>  ChatChannels  => Set<ChatChannel>();
    public DbSet<ChatMessage>  ChatMessages  => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // ── Ministry & Batch Processing ──────────────────────────────────────────
    public DbSet<MinistryBatch> MinistryBatches => Set<MinistryBatch>();
    public DbSet<BatchItem>     BatchItems      => Set<BatchItem>();

    // ── Audit, System & HR ───────────────────────────────────────────────────
    public DbSet<AuditLog>                    AuditLogs                    => Set<AuditLog>();
    public DbSet<OutboxMessage>               OutboxMessages               => Set<OutboxMessage>();
    public DbSet<HRVerificationQueue>         HRVerificationQueue          => Set<HRVerificationQueue>();
    public DbSet<PasswordResetToken>          PasswordResetTokens          => Set<PasswordResetToken>();
    public DbSet<EmailLog>                    EmailLogs                    => Set<EmailLog>();
    public DbSet<SystemConfiguration>         SystemConfigurations         => Set<SystemConfiguration>();
    public DbSet<SystemConfigurationHistory>  SystemConfigurationHistory   => Set<SystemConfigurationHistory>();
    public DbSet<ResearchAttachment>          ResearchAttachments          => Set<ResearchAttachment>();
    public DbSet<FileAccessToken>             FileAccessTokens             => Set<FileAccessToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Auto-discover all IEntityTypeConfiguration<T> implementations in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BOCDbContext).Assembly);
    }
}
