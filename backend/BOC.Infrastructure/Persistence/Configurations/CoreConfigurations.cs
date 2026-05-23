using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BOC.Domain.Entities;
using BOC.Domain.Enums;

namespace BOC.Infrastructure.Persistence.Configurations;

public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.ToTable("AppRoles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.NormalizedName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Description).HasMaxLength(255);
        builder.HasIndex(r => r.Name).IsUnique();
        builder.HasIndex(r => r.NormalizedName).IsUnique();
    }
}

public class DirectorateConfiguration : IEntityTypeConfiguration<Directorate>
{
    public void Configure(EntityTypeBuilder<Directorate> builder)
    {
        builder.ToTable("Directorates");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Code).HasMaxLength(50);
        builder.HasIndex(d => d.Code).IsUnique();
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Code).HasMaxLength(50);
        builder.HasIndex(d => d.Code).IsUnique();

        builder.HasOne(d => d.Directorate)
            .WithMany(dir => dir.Departments)
            .HasForeignKey(d => d.DirectorateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SpecializationConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        builder.ToTable("Specializations");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Code).HasMaxLength(50);
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.HasIndex(s => s.Code).IsUnique();
    }
}

public class EvaluatorSpecializationConfiguration : IEntityTypeConfiguration<EvaluatorSpecialization>
{
    public void Configure(EntityTypeBuilder<EvaluatorSpecialization> builder)
    {
        builder.ToTable("EvaluatorSpecializations");
        builder.HasKey(es => es.Id);
        builder.Property(es => es.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(es => new { es.EvaluatorId, es.SpecializationId }).IsUnique();

        builder.HasOne(es => es.Evaluator)
            .WithMany(u => u.EvaluatorSpecializations)
            .HasForeignKey(es => es.EvaluatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(es => es.Specialization)
            .WithMany(s => s.EvaluatorSpecializations)
            .HasForeignKey(es => es.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserSettingConfiguration : IEntityTypeConfiguration<UserSetting>
{
    public void Configure(EntityTypeBuilder<UserSetting> builder)
    {
        builder.ToTable("UserSettings");
        builder.HasKey(us => us.Id);
        builder.Property(us => us.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(us => us.Language).IsRequired().HasMaxLength(10).HasDefaultValue("AR");
        builder.Property(us => us.Theme).IsRequired().HasMaxLength(20).HasDefaultValue("Light");
        builder.HasIndex(us => us.UserId).IsUnique();

        builder.HasOne(us => us.User)
            .WithOne(u => u.UserSetting)
            .HasForeignKey<UserSetting>(us => us.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ResearchCategoryConfiguration : IEntityTypeConfiguration<ResearchCategory>
{
    public void Configure(EntityTypeBuilder<ResearchCategory> builder)
    {
        builder.ToTable("ResearchCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(c => c.Specialization)
            .WithMany(s => s.ResearchCategories)
            .HasForeignKey(c => c.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.ToTable("Meetings");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(m => m.MeetingNumber).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Title).HasMaxLength(300);
        builder.Property(m => m.Location).HasMaxLength(200);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(50).HasDefaultValue(MeetingStatus.Scheduled);
        builder.HasIndex(m => m.MeetingNumber).IsUnique();

        builder.HasOne(m => m.CreatedBy)
            .WithMany(u => u.CreatedMeetings)
            .HasForeignKey(m => m.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MeetingAgendaConfiguration : IEntityTypeConfiguration<MeetingAgenda>
{
    public void Configure(EntityTypeBuilder<MeetingAgenda> builder)
    {
        builder.ToTable("MeetingAgendas");
        builder.HasKey(ma => ma.Id);
        builder.Property(ma => ma.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(ma => ma.DocumentPath).IsRequired().HasMaxLength(500);

        builder.HasOne(ma => ma.Meeting)
            .WithMany(m => m.Agendas)
            .HasForeignKey(ma => ma.MeetingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ma => ma.UploadedBy)
            .WithMany()
            .HasForeignKey(ma => ma.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FreezeEventConfiguration : IEntityTypeConfiguration<FreezeEvent>
{
    public void Configure(EntityTypeBuilder<FreezeEvent> builder)
    {
        builder.ToTable("FreezeEvents");
        builder.HasKey(fe => fe.Id);
        builder.Property(fe => fe.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.HasOne(fe => fe.MeetingMinutes)
            .WithMany(m => m.FreezeEvents)
            .HasForeignKey(fe => fe.MeetingMinutesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fe => fe.FrozenBy)
            .WithMany()
            .HasForeignKey(fe => fe.FrozenById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ResearchVersionConfiguration : IEntityTypeConfiguration<ResearchVersion>
{
    public void Configure(EntityTypeBuilder<ResearchVersion> builder)
    {
        builder.ToTable("ResearchVersions");
        builder.HasKey(rv => rv.Id);
        builder.Property(rv => rv.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(rv => rv.DocumentPath).IsRequired().HasMaxLength(500);
        builder.Property(rv => rv.ChangeSummary).HasMaxLength(1000);

        builder.HasOne(rv => rv.ResearchPaper)
            .WithMany(r => r.Versions)
            .HasForeignKey(rv => rv.ResearchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ResearchCorrectionConfiguration : IEntityTypeConfiguration<ResearchCorrection>
{
    public void Configure(EntityTypeBuilder<ResearchCorrection> builder)
    {
        builder.ToTable("ResearchCorrections");
        builder.HasKey(rc => rc.Id);
        builder.Property(rc => rc.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(rc => rc.DocumentPath).IsRequired().HasMaxLength(500);

        builder.HasOne(rc => rc.ResearchPaper)
            .WithMany(r => r.Corrections)
            .HasForeignKey(rc => rc.ResearchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SubstitutionConfiguration : IEntityTypeConfiguration<Substitution>
{
    public void Configure(EntityTypeBuilder<Substitution> builder)
    {
        builder.ToTable("Substitutions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.HasOne(s => s.OriginalResearch)
            .WithMany(r => r.OriginalSubstitutions)
            .HasForeignKey(s => s.OriginalResearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.NewResearch)
            .WithMany(r => r.NewSubstitutions)
            .HasForeignKey(s => s.NewResearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SubstitutedBy)
            .WithMany()
            .HasForeignKey(s => s.SubstitutedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PlagiarismLockoutConfiguration : IEntityTypeConfiguration<PlagiarismLockout>
{
    public void Configure(EntityTypeBuilder<PlagiarismLockout> builder)
    {
        builder.ToTable("PlagiarismLockouts");
        builder.HasKey(pl => pl.Id);
        builder.Property(pl => pl.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(pl => pl.Reason).HasMaxLength(500);

        builder.HasOne(pl => pl.ResearchPaper)
            .WithMany(r => r.PlagiarismLockouts)
            .HasForeignKey(pl => pl.ResearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pl => pl.DetectedBy)
            .WithMany()
            .HasForeignKey(pl => pl.DetectedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PlagiarismOverrideJustificationConfiguration : IEntityTypeConfiguration<PlagiarismOverrideJustification>
{
    public void Configure(EntityTypeBuilder<PlagiarismOverrideJustification> builder)
    {
        builder.ToTable("PlagiarismOverrideJustifications");
        builder.HasKey(poj => poj.Id);
        builder.Property(poj => poj.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(poj => poj.JustificationDocumentPath).IsRequired().HasMaxLength(500);

        builder.HasOne(poj => poj.Lockout)
            .WithMany(pl => pl.OverrideJustifications)
            .HasForeignKey(poj => poj.LockoutId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(poj => poj.LiftedBy)
            .WithMany()
            .HasForeignKey(poj => poj.LiftedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.ToTable("Evaluations");
        builder.HasKey(ev => ev.Id);
        builder.Property(ev => ev.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(ev => ev.Score).HasPrecision(5, 2);

        builder.HasOne(ev => ev.Assignment)
            .WithMany(ea => ea.Evaluations)
            .HasForeignKey(ev => ev.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChairmanScoreConfiguration : IEntityTypeConfiguration<ChairmanScore>
{
    public void Configure(EntityTypeBuilder<ChairmanScore> builder)
    {
        builder.ToTable("ChairmanScores");
        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(cs => cs.Score).HasPrecision(5, 2);

        builder.HasOne(cs => cs.ResearchPaper)
            .WithMany(r => r.ChairmanScores)
            .HasForeignKey(cs => cs.ResearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.Chairman)
            .WithMany()
            .HasForeignKey(cs => cs.ChairmanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.MeetingMinutes)
            .WithMany(m => m.ChairmanScores)
            .HasForeignKey(cs => cs.MeetingMinutesId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MeetingRSVPConfiguration : IEntityTypeConfiguration<MeetingRSVP>
{
    public void Configure(EntityTypeBuilder<MeetingRSVP> builder)
    {
        builder.ToTable("MeetingRSVPs");
        builder.HasKey(mr => mr.Id);
        builder.Property(mr => mr.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(mr => mr.Response).IsRequired().HasMaxLength(50);
        builder.Property(mr => mr.Justification).IsRequired().HasMaxLength(500).HasDefaultValue("");

        builder.HasOne(mr => mr.Meeting)
            .WithMany(m => m.RSVPs)
            .HasForeignKey(mr => mr.MeetingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Member)
            .WithMany()
            .HasForeignKey(mr => mr.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MeetingAttendanceConfiguration : IEntityTypeConfiguration<MeetingAttendance>
{
    public void Configure(EntityTypeBuilder<MeetingAttendance> builder)
    {
        builder.ToTable("MeetingAttendance");
        builder.HasKey(ma => ma.Id);
        builder.Property(ma => ma.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.HasOne(ma => ma.Meeting)
            .WithMany(m => m.Attendances)
            .HasForeignKey(ma => ma.MeetingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ma => ma.Member)
            .WithMany()
            .HasForeignKey(ma => ma.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChatChannelConfiguration : IEntityTypeConfiguration<ChatChannel>
{
    public void Configure(EntityTypeBuilder<ChatChannel> builder)
    {
        builder.ToTable("ChatChannels");
        builder.HasKey(cc => cc.Id);
        builder.Property(cc => cc.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(cc => cc.Name).IsRequired().HasMaxLength(200);
        builder.Property(cc => cc.ChannelType).HasConversion<string>().HasMaxLength(50);

        builder.HasOne(cc => cc.CreatedBy)
            .WithMany()
            .HasForeignKey(cc => cc.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(cm => cm.Id);
        builder.Property(cm => cm.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(cm => cm.ChannelType).HasConversion<string>().HasMaxLength(50);

        builder.HasOne(cm => cm.Channel)
            .WithMany(cc => cc.Messages)
            .HasForeignKey(cm => cm.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.Sender)
            .WithMany()
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.Receiver)
            .WithMany()
            .HasForeignKey(cm => cm.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.RelatedResearch)
            .WithMany()
            .HasForeignKey(cm => cm.RelatedResearchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(n => n.Type).IsRequired().HasMaxLength(100);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MinistryBatchConfiguration : IEntityTypeConfiguration<MinistryBatch>
{
    public void Configure(EntityTypeBuilder<MinistryBatch> builder)
    {
        builder.ToTable("MinistryBatches");
        builder.HasKey(mb => mb.Id);
        builder.Property(mb => mb.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(mb => mb.BatchNumber).IsRequired().HasMaxLength(100);
        builder.Property(mb => mb.Status).HasConversion<string>().HasMaxLength(50).HasDefaultValue(MinistryBatchStatus.Pending);
        builder.HasIndex(mb => mb.BatchNumber).IsUnique();
    }
}

public class BatchItemConfiguration : IEntityTypeConfiguration<BatchItem>
{
    public void Configure(EntityTypeBuilder<BatchItem> builder)
    {
        builder.ToTable("BatchItems");
        builder.HasKey(bi => bi.Id);
        builder.Property(bi => bi.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(bi => bi.MinistryDecision).HasMaxLength(50);

        builder.HasOne(bi => bi.Batch)
            .WithMany(mb => mb.Items)
            .HasForeignKey(bi => bi.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bi => bi.ResearchPaper)
            .WithMany()
            .HasForeignKey(bi => bi.ResearchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(om => om.Id);
        builder.Property(om => om.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(om => om.EventType).IsRequired().HasMaxLength(200);
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");
        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(pr => pr.TokenHash).IsRequired().HasMaxLength(500);

        builder.HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");
        builder.HasKey(el => el.Id);
        builder.Property(el => el.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(el => el.RecipientEmail).IsRequired().HasMaxLength(200);
        builder.Property(el => el.Subject).IsRequired().HasMaxLength(500);
        builder.Property(el => el.TemplateType).HasMaxLength(100);
        builder.Property(el => el.DeliveryStatus).HasConversion<string>().HasMaxLength(50).HasDefaultValue(EmailDeliveryStatus.Pending);
    }
}

public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("SystemConfigurations");
        builder.HasKey(sc => sc.Id);
        builder.Property(sc => sc.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(sc => sc.ConfigKey).IsRequired().HasMaxLength(100);
        builder.Property(sc => sc.Description).HasMaxLength(500);
        builder.HasIndex(sc => sc.ConfigKey).IsUnique();

        builder.HasOne(sc => sc.ModifiedBy)
            .WithMany()
            .HasForeignKey(sc => sc.ModifiedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SystemConfigurationHistoryConfiguration : IEntityTypeConfiguration<SystemConfigurationHistory>
{
    public void Configure(EntityTypeBuilder<SystemConfigurationHistory> builder)
    {
        builder.ToTable("SystemConfigurationHistory");
        builder.HasKey(sch => sch.Id);
        builder.Property(sch => sch.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(sch => sch.ConfigKey).IsRequired().HasMaxLength(100);

        builder.HasOne(sch => sch.ModifiedBy)
            .WithMany()
            .HasForeignKey(sch => sch.ModifiedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ResearchAttachmentConfiguration : IEntityTypeConfiguration<ResearchAttachment>
{
    public void Configure(EntityTypeBuilder<ResearchAttachment> builder)
    {
        builder.ToTable("ResearchAttachments");
        builder.HasKey(ra => ra.Id);
        builder.Property(ra => ra.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(ra => ra.FileName).IsRequired().HasMaxLength(255);
        builder.Property(ra => ra.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(ra => ra.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(ra => ra.Sha256Hash).IsRequired().HasMaxLength(64);

        builder.HasOne(ra => ra.ResearchPaper)
            .WithMany(r => r.Attachments)
            .HasForeignKey(ra => ra.ResearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ra => ra.UploadedBy)
            .WithMany()
            .HasForeignKey(ra => ra.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FileAccessTokenConfiguration : IEntityTypeConfiguration<FileAccessToken>
{
    public void Configure(EntityTypeBuilder<FileAccessToken> builder)
    {
        builder.ToTable("FileAccessTokens");
        builder.HasKey(fa => fa.Id);
        builder.Property(fa => fa.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(fa => fa.TokenHash).IsRequired().HasMaxLength(500);

        builder.HasOne(fa => fa.Attachment)
            .WithMany(ra => ra.FileAccessTokens)
            .HasForeignKey(fa => fa.AttachmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fa => fa.CreatedBy)
            .WithMany()
            .HasForeignKey(fa => fa.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
