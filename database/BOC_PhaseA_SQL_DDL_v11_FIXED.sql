-- =================================================================
-- BOC Research Evaluation & Workflow Management System (v11)
-- PHASE A: DATABASE LAYER — SQL DDL SCRIPT (FIXED)
-- Distributed Architecture | EF Core Ready | Production-Grade
-- =================================================================
-- FIXES APPLIED:
--   1. IsSLABreached: Changed from computed to BIT with DEFAULT 0
--      (computed with GETUTCDATE() is non-deterministic)
--   2. EvaluatorAssignments: Moved BEFORE Evaluations to resolve FK dependency
--   3. Filtered indexes on computed columns: Rewritten to use base column expressions
--   4. FileAccessTokens filtered index: Fixed WHERE clause syntax
-- =================================================================

IF DB_ID(N'BOC_Research_Evaluation') IS NOT NULL
BEGIN
    ALTER DATABASE [BOC_Research_Evaluation] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [BOC_Research_Evaluation];
END
GO

CREATE DATABASE [BOC_Research_Evaluation]
    COLLATE Arabic_CI_AI_KS_WS;
GO

USE [BOC_Research_Evaluation];
GO

-- =================================================================
-- SECTION 1: CORE IDENTITY & DIRECTORY TABLES
-- =================================================================

CREATE TABLE [dbo].[AppRoles] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Name]              NVARCHAR(100)       NOT NULL,
    [NormalizedName]    NVARCHAR(100)       NOT NULL,
    [Description]       NVARCHAR(255)       NULL,
    [PermissionsJson]   NVARCHAR(MAX)       NULL,
    [IsActive]          BIT                 NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_AppRoles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_AppRoles_Name] UNIQUE NONCLUSTERED ([Name]),
    CONSTRAINT [UQ_AppRoles_NormalizedName] UNIQUE NONCLUSTERED ([NormalizedName])
);
GO

CREATE TABLE [dbo].[Directorates] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Name]              NVARCHAR(200)       NOT NULL,
    [Code]              NVARCHAR(50)        NULL,
    [IsActive]          BIT                 NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Directorates] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Directorates_Code] UNIQUE NONCLUSTERED ([Code])
);
GO

CREATE TABLE [dbo].[Departments] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [DirectorateId]     UNIQUEIDENTIFIER    NOT NULL,
    [Name]              NVARCHAR(200)       NOT NULL,
    [Code]              NVARCHAR(50)        NULL,
    [IsActive]          BIT                 NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Departments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Departments_Directorates] FOREIGN KEY ([DirectorateId]) 
        REFERENCES [dbo].[Directorates]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_Departments_Code] UNIQUE NONCLUSTERED ([Code])
);
GO

CREATE TABLE [dbo].[Specializations] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Name]              NVARCHAR(200)       NOT NULL,
    [Code]              NVARCHAR(50)        NULL,
    [Description]       NVARCHAR(500)       NULL,
    [IsActive]          BIT                 NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Specializations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Specializations_Code] UNIQUE NONCLUSTERED ([Code])
);
GO

CREATE TABLE [dbo].[AppUsers] (
    [Id]                    UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [EmployeeID]            NVARCHAR(50)        NOT NULL,
    [NationalID]            NVARCHAR(50)        NOT NULL,
    [FullName]              NVARCHAR(200)       NOT NULL,
    [Email]                 NVARCHAR(200)       NOT NULL,
    [NormalizedEmail]       NVARCHAR(200)       NULL,
    [PasswordHash]          NVARCHAR(500)       NULL,
    [SecurityStamp]         NVARCHAR(500)       NULL,
    [ConcurrencyStamp]      NVARCHAR(500)       NULL,
    [PhoneNumber]           NVARCHAR(50)        NULL,
    [RoleId]                UNIQUEIDENTIFIER    NOT NULL,
    [DepartmentId]          UNIQUEIDENTIFIER    NULL,
    [DirectorateId]         UNIQUEIDENTIFIER    NULL,
    [BirthDate]             DATE                NULL,
    [EvaluatorStatus]       NVARCHAR(50)        NOT NULL DEFAULT N'Active',
    [AccountStatus]         NVARCHAR(50)        NOT NULL DEFAULT N'Pending_HR_Verification',
    [IsEmailConfirmed]      BIT                 NOT NULL DEFAULT 0,
    [LockoutEnd]            DATETIME2(7)        NULL,
    [AccessFailedCount]     INT                 NOT NULL DEFAULT 0,
    [RefreshTokenHash]      NVARCHAR(500)       NULL,
    [RefreshTokenExpires]   DATETIME2(7)        NULL,
    [TwoFactorEnabled]      BIT                 NOT NULL DEFAULT 0,
    [TwoFactorSecret]       NVARCHAR(500)       NULL,
    [DeviceFingerprint]     NVARCHAR(500)       NULL,
    [LastLoginAt]           DATETIME2(7)        NULL,
    [CreatedAt]             DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]            DATETIME2(7)        NULL,

    CONSTRAINT [PK_AppUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppUsers_AppRoles] FOREIGN KEY ([RoleId]) 
        REFERENCES [dbo].[AppRoles]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AppUsers_Departments] FOREIGN KEY ([DepartmentId]) 
        REFERENCES [dbo].[Departments]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AppUsers_Directorates] FOREIGN KEY ([DirectorateId]) 
        REFERENCES [dbo].[Directorates]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_AppUsers_EmployeeID] UNIQUE NONCLUSTERED ([EmployeeID]),
    CONSTRAINT [UQ_AppUsers_NationalID] UNIQUE NONCLUSTERED ([NationalID]),
    CONSTRAINT [UQ_AppUsers_Email] UNIQUE NONCLUSTERED ([Email]),
    CONSTRAINT [CHK_AppUsers_EvaluatorStatus] CHECK ([EvaluatorStatus] IN (N'Active', N'Suspended_Permanently', N'Suspended_Temporarily')),
    CONSTRAINT [CHK_AppUsers_AccountStatus] CHECK ([AccountStatus] IN (N'Pending_HR_Verification', N'Active', N'Locked', N'Retired', N'Deceased'))
);
GO

CREATE TABLE [dbo].[EvaluatorSpecializations] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [EvaluatorId]       UNIQUEIDENTIFIER    NOT NULL,
    [SpecializationId]  UNIQUEIDENTIFIER    NOT NULL,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_EvaluatorSpecializations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_EvaluatorSpecializations_AppUsers] FOREIGN KEY ([EvaluatorId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EvaluatorSpecializations_Specializations] FOREIGN KEY ([SpecializationId]) 
        REFERENCES [dbo].[Specializations]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_EvaluatorSpecializations] UNIQUE NONCLUSTERED ([EvaluatorId], [SpecializationId])
);
GO

CREATE TABLE [dbo].[UserSettings] (
    [Id]                        UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [UserId]                    UNIQUEIDENTIFIER    NOT NULL,
    [Language]                  NVARCHAR(10)        NOT NULL DEFAULT N'AR',
    [Theme]                     NVARCHAR(20)        NOT NULL DEFAULT N'Light',
    [EmailNotificationsEnabled] BIT                 NOT NULL DEFAULT 1,
    [CreatedAt]                 DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]                DATETIME2(7)        NULL,

    CONSTRAINT [PK_UserSettings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserSettings_AppUsers] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_UserSettings_UserId] UNIQUE NONCLUSTERED ([UserId]),
    CONSTRAINT [CHK_UserSettings_Language] CHECK ([Language] IN (N'AR', N'EN')),
    CONSTRAINT [CHK_UserSettings_Theme] CHECK ([Theme] IN (N'Light', N'Dark'))
);
GO

CREATE TABLE [dbo].[DelegatedRoles] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [FromUserId]    UNIQUEIDENTIFIER    NOT NULL,
    [ToUserId]      UNIQUEIDENTIFIER    NOT NULL,
    [PermissionsMask] BIGINT              NOT NULL DEFAULT 0,
    [ValidFrom]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ValidUntil]    DATETIME2(7)        NOT NULL,
    [CreatedById]   UNIQUEIDENTIFIER    NOT NULL,
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_DelegatedRoles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DelegatedRoles_FromUser] FOREIGN KEY ([FromUserId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DelegatedRoles_ToUser] FOREIGN KEY ([ToUserId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DelegatedRoles_CreatedBy] FOREIGN KEY ([CreatedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

-- =================================================================
-- SECTION 2: RESEARCH LIFECYCLE CORE
-- =================================================================

CREATE TABLE [dbo].[ResearchCategories] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Name]              NVARCHAR(200)       NOT NULL,
    [SpecializationId]  UNIQUEIDENTIFIER    NOT NULL,
    [IsActive]          BIT                 NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_ResearchCategories] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ResearchCategories_Specializations] FOREIGN KEY ([SpecializationId]) 
        REFERENCES [dbo].[Specializations]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[Meetings] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [MeetingNumber]     NVARCHAR(100)       NOT NULL,
    [Title]             NVARCHAR(300)       NULL,
    [ScheduledDate]     DATETIME2(7)        NOT NULL,
    [Location]          NVARCHAR(200)       NULL,
    [Status]            NVARCHAR(50)        NOT NULL DEFAULT N'Scheduled',
    [CreatedById]       UNIQUEIDENTIFIER    NOT NULL,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]        DATETIME2(7)        NULL,

    CONSTRAINT [PK_Meetings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Meetings_AppUsers] FOREIGN KEY ([CreatedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_Meetings_MeetingNumber] UNIQUE NONCLUSTERED ([MeetingNumber]),
    CONSTRAINT [CHK_Meetings_Status] CHECK ([Status] IN (N'Scheduled', N'InProgress', N'Completed', N'Cancelled'))
);
GO

CREATE TABLE [dbo].[MeetingAgendas] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [MeetingId]     UNIQUEIDENTIFIER    NOT NULL,
    [DocumentPath]  NVARCHAR(500)       NOT NULL,
    [UploadedById]  UNIQUEIDENTIFIER    NOT NULL,
    [UploadedAt]    DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_MeetingAgendas] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetingAgendas_Meetings] FOREIGN KEY ([MeetingId]) 
        REFERENCES [dbo].[Meetings]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MeetingAgendas_AppUsers] FOREIGN KEY ([UploadedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[MeetingMinutes] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [MeetingId]         UNIQUEIDENTIFIER    NOT NULL,
    [MinutesNumber]     NVARCHAR(100)       NOT NULL,
    [Content]           NVARCHAR(MAX)       NULL,
    [Status]            NVARCHAR(50)        NOT NULL DEFAULT N'Draft',
    [SignedDate]        DATETIME2(7)        NULL,
    [SignedById]        UNIQUEIDENTIFIER    NULL,
    [IsFrozen]          AS (CASE WHEN [Status] = N'Minutes_Frozen' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END) PERSISTED,
    [RowVersion]        ROWVERSION          NOT NULL,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]        DATETIME2(7)        NULL,

    CONSTRAINT [PK_MeetingMinutes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetingMinutes_Meetings] FOREIGN KEY ([MeetingId]) 
        REFERENCES [dbo].[Meetings]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MeetingMinutes_AppUsers] FOREIGN KEY ([SignedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_MeetingMinutes_Number] UNIQUE NONCLUSTERED ([MinutesNumber]),
    CONSTRAINT [CHK_MeetingMinutes_Status] CHECK ([Status] IN (N'Draft', N'Signed', N'Minutes_Frozen'))
);
GO

CREATE TABLE [dbo].[FreezeEvents] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [MeetingMinutesId]  UNIQUEIDENTIFIER    NOT NULL,
    [FrozenById]        UNIQUEIDENTIFIER    NOT NULL,
    [FrozenAt]          DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_FreezeEvents] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_FreezeEvents_MeetingMinutes] FOREIGN KEY ([MeetingMinutesId]) 
        REFERENCES [dbo].[MeetingMinutes]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FreezeEvents_AppUsers] FOREIGN KEY ([FrozenById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[ResearchPapers] (
    [Id]                    UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [TrackingNumber]        NVARCHAR(100)       NOT NULL,
    [Title]                 NVARCHAR(500)       NOT NULL,
    [Abstract]              NVARCHAR(MAX)       NULL,
    [ResearcherId]          UNIQUEIDENTIFIER    NOT NULL,
    [CategoryId]            UNIQUEIDENTIFIER    NULL,
    [State]                 NVARCHAR(50)        NOT NULL DEFAULT N'Draft',
    [ReplacedResearchId]    UNIQUEIDENTIFIER    NULL,
    [MeetingId]             UNIQUEIDENTIFIER    NULL,
    [MeetingMinutesId]      UNIQUEIDENTIFIER    NULL,
    [DepartmentId]          UNIQUEIDENTIFIER    NOT NULL,
    [DirectorateId]         UNIQUEIDENTIFIER    NOT NULL,
    [FinalScore]            DECIMAL(5,2)        NULL,
    [IsArchived]            AS (CASE WHEN [State] = N'Archived' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END) PERSISTED,
    [SubmissionDate]        DATETIME2(7)        NULL,
    [RowVersion]            ROWVERSION          NOT NULL,
    [CreatedAt]             DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]            DATETIME2(7)        NULL,

    CONSTRAINT [PK_ResearchPapers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ResearchPapers_AppUsers] FOREIGN KEY ([ResearcherId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResearchPapers_Self_Replace] FOREIGN KEY ([ReplacedResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResearchPapers_Categories] FOREIGN KEY ([CategoryId]) 
        REFERENCES [dbo].[ResearchCategories]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResearchPapers_Meetings] FOREIGN KEY ([MeetingId]) 
        REFERENCES [dbo].[Meetings]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResearchPapers_MeetingMinutes] FOREIGN KEY ([MeetingMinutesId]) 
        REFERENCES [dbo].[MeetingMinutes]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResearchPapers_Departments] FOREIGN KEY ([DepartmentId]) 
        REFERENCES [dbo].[Departments]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResearchPapers_Directorates] FOREIGN KEY ([DirectorateId]) 
        REFERENCES [dbo].[Directorates]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_ResearchPapers_TrackingNumber] UNIQUE NONCLUSTERED ([TrackingNumber]),
    CONSTRAINT [CHK_ResearchPapers_State] CHECK ([State] IN (
        N'Draft', N'Pending_Secretary_Screening', N'Non_Compliant_Returned', 
        N'Incoming_Triage_Queue', N'Dispatched_To_Evaluators', N'Pending_Chairman_Grading', 
        N'Substituted', N'Suspended_Plagiarism_Lockout', N'Force_Majeure_Retired', 
        N'Force_Majeure_Deceased', N'Ministry_Batch_Transit', N'Pass_Approved', 
        N'Fail_Rejected', N'Archived'
    ))
);
GO

CREATE TABLE [dbo].[ResearchVersions] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ResearchId]    UNIQUEIDENTIFIER    NOT NULL,
    [VersionNumber] INT                 NOT NULL DEFAULT 1,
    [DocumentPath]  NVARCHAR(500)       NOT NULL,
    [ChangeSummary] NVARCHAR(1000)      NULL,
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_ResearchVersions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ResearchVersions_Research] FOREIGN KEY ([ResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[ResearchCorrections] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ResearchId]    UNIQUEIDENTIFIER    NOT NULL,
    [CorrectionRound]   INT             NOT NULL DEFAULT 1,
    [DocumentPath]  NVARCHAR(500)       NOT NULL,
    [SecretaryNotes]    NVARCHAR(MAX)   NULL,
    [SubmittedAt]   DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_ResearchCorrections] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ResearchCorrections_Research] FOREIGN KEY ([ResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[Substitutions] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OriginalResearchId]    UNIQUEIDENTIFIER    NOT NULL,
    [NewResearchId]         UNIQUEIDENTIFIER    NOT NULL,
    [SubstitutedById]       UNIQUEIDENTIFIER    NOT NULL,
    [Justification]         NVARCHAR(MAX)       NULL,
    [SubstitutedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [CreatedAt]             DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Substitutions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Substitutions_Original] FOREIGN KEY ([OriginalResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Substitutions_New] FOREIGN KEY ([NewResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Substitutions_AppUsers] FOREIGN KEY ([SubstitutedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[PlagiarismLockouts] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ResearchId]        UNIQUEIDENTIFIER    NOT NULL,
    [LockedUntil]       DATETIME2(7)        NOT NULL,
    [Reason]            NVARCHAR(500)       NULL,
    [DetectedById]      UNIQUEIDENTIFIER    NOT NULL,
    [DetectedAt]        DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_PlagiarismLockouts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PlagiarismLockouts_Research] FOREIGN KEY ([ResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlagiarismLockouts_AppUsers] FOREIGN KEY ([DetectedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[PlagiarismOverrideJustifications] (
    [Id]                        UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [LockoutId]                 UNIQUEIDENTIFIER    NOT NULL,
    [LiftedById]                UNIQUEIDENTIFIER    NOT NULL,
    [JustificationDocumentPath] NVARCHAR(500)       NOT NULL,
    [LiftedAt]                  DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [CreatedAt]                 DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_PlagiarismOverrideJustifications] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PlagiarismOverrideJustifications_Lockout] FOREIGN KEY ([LockoutId]) 
        REFERENCES [dbo].[PlagiarismLockouts]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlagiarismOverrideJustifications_AppUsers] FOREIGN KEY ([LiftedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

-- =================================================================
-- SECTION 3: EVALUATION ENGINE (MOVED BEFORE SECTION 4 REFERENCES)
-- =================================================================

CREATE TABLE [dbo].[EvaluatorAssignments] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ResearchId]        UNIQUEIDENTIFIER    NOT NULL,
    [EvaluatorId]       UNIQUEIDENTIFIER    NOT NULL,
    [AssignedById]      UNIQUEIDENTIFIER    NOT NULL,
    [AssignedDate]      DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [DueDate]           DATETIME2(7)        NOT NULL,
    [SubmittedDate]     DATETIME2(7)        NULL,
    [ReminderCount]     INT                 NOT NULL DEFAULT 0,
    [Status]            NVARCHAR(50)        NOT NULL DEFAULT N'Active',
    [IsSLABreached]     BIT                 NOT NULL DEFAULT 0,
    [RowVersion]        ROWVERSION          NOT NULL,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]        DATETIME2(7)        NULL,

    CONSTRAINT [PK_EvaluatorAssignments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_EvaluatorAssignments_Research] FOREIGN KEY ([ResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EvaluatorAssignments_Evaluator] FOREIGN KEY ([EvaluatorId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EvaluatorAssignments_Assigner] FOREIGN KEY ([AssignedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CHK_EvaluatorAssignments_Status] CHECK ([Status] IN (N'Active', N'Substituted', N'Expired'))
);
GO

CREATE TABLE [dbo].[Evaluations] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [AssignmentId]  UNIQUEIDENTIFIER    NOT NULL,
    [Score]         DECIMAL(5,2)        NOT NULL,
    [Comments]      NVARCHAR(MAX)       NULL,
    [SubmittedAt]   DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Evaluations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Evaluations_Assignment] FOREIGN KEY ([AssignmentId]) 
        REFERENCES [dbo].[EvaluatorAssignments]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CHK_Evaluations_Score] CHECK ([Score] >= 0 AND [Score] <= 100)
);
GO

CREATE TABLE [dbo].[ChairmanScores] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ResearchId]        UNIQUEIDENTIFIER    NOT NULL,
    [ChairmanId]        UNIQUEIDENTIFIER    NOT NULL,
    [Score]             DECIMAL(5,2)        NOT NULL,
    [MeetingMinutesId]  UNIQUEIDENTIFIER    NOT NULL,
    [Comments]          NVARCHAR(MAX)       NULL,
    [SubmittedAt]       DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_ChairmanScores] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ChairmanScores_Research] FOREIGN KEY ([ResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChairmanScores_Chairman] FOREIGN KEY ([ChairmanId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChairmanScores_Minutes] FOREIGN KEY ([MeetingMinutesId]) 
        REFERENCES [dbo].[MeetingMinutes]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CHK_ChairmanScores_Score] CHECK ([Score] >= 0 AND [Score] <= 30)
);
GO

-- =================================================================
-- SECTION 4: TRIAGE MAPPINGS (AFTER EvaluatorAssignments)
-- =================================================================

CREATE TABLE [dbo].[TriageMappings] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ResearchId]    UNIQUEIDENTIFIER    NOT NULL,
    [MappedById]    UNIQUEIDENTIFIER    NOT NULL,
    [EvaluatorId]   UNIQUEIDENTIFIER    NULL,
    [MemberId]      UNIQUEIDENTIFIER    NULL,
    [IsFinalized]   BIT                 NOT NULL DEFAULT 0,
    [MappedAt]      DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]    DATETIME2(7)        NULL,
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [RowVersion]    ROWVERSION          NOT NULL,

    CONSTRAINT [PK_TriageMappings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TriageMappings_Research] FOREIGN KEY ([ResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TriageMappings_MappedBy] FOREIGN KEY ([MappedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TriageMappings_Evaluator] FOREIGN KEY ([EvaluatorId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TriageMappings_Member] FOREIGN KEY ([MemberId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

-- =================================================================
-- SECTION 5: MEETINGS & GOVERNANCE
-- =================================================================

CREATE TABLE [dbo].[MeetingRSVPs] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [MeetingId]     UNIQUEIDENTIFIER    NOT NULL,
    [MemberId]      UNIQUEIDENTIFIER    NOT NULL,
    [Response]      NVARCHAR(50)        NOT NULL,
    [Justification] NVARCHAR(500)       NOT NULL DEFAULT N'',
    [RespondedAt]   DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_MeetingRSVPs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetingRSVPs_Meetings] FOREIGN KEY ([MeetingId]) 
        REFERENCES [dbo].[Meetings]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MeetingRSVPs_Members] FOREIGN KEY ([MemberId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CHK_MeetingRSVPs_Response] CHECK ([Response] IN (N'Accept', N'Decline'))
);
GO

CREATE TABLE [dbo].[MeetingAttendance] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [MeetingId]         UNIQUEIDENTIFIER    NOT NULL,
    [MemberId]          UNIQUEIDENTIFIER    NOT NULL,
    [Attended]          BIT                 NOT NULL DEFAULT 0,
    [AttendanceMarkedAt]    DATETIME2(7)    NULL,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_MeetingAttendance] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetingAttendance_Meetings] FOREIGN KEY ([MeetingId]) 
        REFERENCES [dbo].[Meetings]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MeetingAttendance_Members] FOREIGN KEY ([MemberId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[Votes] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [MeetingId]     UNIQUEIDENTIFIER    NOT NULL,
    [ResearchId]    UNIQUEIDENTIFIER    NOT NULL,
    [MemberId]      UNIQUEIDENTIFIER    NOT NULL,
    [VoteValue]     NVARCHAR(50)        NOT NULL,
    [VotedAt]       DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Votes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Votes_Meetings] FOREIGN KEY ([MeetingId]) 
        REFERENCES [dbo].[Meetings]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Votes_Research] FOREIGN KEY ([ResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Votes_Members] FOREIGN KEY ([MemberId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CHK_Votes_VoteValue] CHECK ([VoteValue] IN (N'Approve', N'Reject', N'Abstain'))
);
GO

-- =================================================================
-- SECTION 6: COLLABORATION & NOTIFICATIONS
-- =================================================================

CREATE TABLE [dbo].[ChatChannels] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Name]          NVARCHAR(200)       NOT NULL,
    [ChannelType]   NVARCHAR(50)        NOT NULL,
    [CreatedById]   UNIQUEIDENTIFIER    NOT NULL,
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_ChatChannels] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ChatChannels_AppUsers] FOREIGN KEY ([CreatedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CHK_ChatChannels_ChannelType] CHECK ([ChannelType] IN (N'Committee_Internal', N'Evaluator_Admin_Anonymous'))
);
GO

CREATE TABLE [dbo].[ChatMessages] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ChannelId]         UNIQUEIDENTIFIER    NOT NULL,
    [ChannelType]       NVARCHAR(50)        NOT NULL,
    [SenderId]          UNIQUEIDENTIFIER    NOT NULL,
    [ReceiverId]        UNIQUEIDENTIFIER    NULL,
    [Content]           NVARCHAR(MAX)       NOT NULL,
    [SentAt]            DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [IsAnonymous]       BIT                 NOT NULL DEFAULT 0,
    [IsRead]            BIT                 NOT NULL DEFAULT 0,
    [RelatedResearchId] UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_ChatMessages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ChatChannels_Channel] FOREIGN KEY ([ChannelId]) 
        REFERENCES [dbo].[ChatChannels]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatMessages_Sender] FOREIGN KEY ([SenderId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatMessages_Receiver] FOREIGN KEY ([ReceiverId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatMessages_Research] FOREIGN KEY ([RelatedResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CHK_ChatMessages_ChannelType] CHECK ([ChannelType] IN (N'Committee_Internal', N'Evaluator_Admin_Anonymous'))
);
GO

CREATE TABLE [dbo].[Notifications] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [UserId]            UNIQUEIDENTIFIER    NOT NULL,
    [Type]              NVARCHAR(100)       NOT NULL,
    [Title]             NVARCHAR(200)       NOT NULL,
    [Message]           NVARCHAR(MAX)       NOT NULL,
    [RelatedEntityId]   UNIQUEIDENTIFIER    NULL,
    [RelatedEntityType] NVARCHAR(100)       NULL,
    [IsRead]            BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Notifications_AppUsers] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

-- =================================================================
-- SECTION 7: MINISTRY & BATCH PROCESSING
-- =================================================================

CREATE TABLE [dbo].[MinistryBatches] (
    [Id]                    UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [BatchNumber]           NVARCHAR(100)       NOT NULL,
    [SentDate]              DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [MinistryResponseJson]  NVARCHAR(MAX)       NULL,
    [Status]                NVARCHAR(50)        NOT NULL DEFAULT N'Pending',
    [CreatedAt]             DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]            DATETIME2(7)        NULL,

    CONSTRAINT [PK_MinistryBatches] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_MinistryBatches_BatchNumber] UNIQUE NONCLUSTERED ([BatchNumber]),
    CONSTRAINT [CHK_MinistryBatches_Status] CHECK ([Status] IN (N'Pending', N'Approved', N'Rejected'))
);
GO

CREATE TABLE [dbo].[BatchItems] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [BatchId]           UNIQUEIDENTIFIER    NOT NULL,
    [ResearchId]        UNIQUEIDENTIFIER    NOT NULL,
    [MinistryDecision]  NVARCHAR(50)        NULL,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_BatchItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_BatchItems_Batch] FOREIGN KEY ([BatchId]) 
        REFERENCES [dbo].[MinistryBatches]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BatchItems_Research] FOREIGN KEY ([ResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION
);
GO

-- =================================================================
-- SECTION 8: AUDIT, SYSTEM & HR
-- =================================================================

CREATE TABLE [dbo].[AuditLogs] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Timestamp]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [OperatorEmployeeID] NVARCHAR(50)        NOT NULL,
    [ActionType]        NVARCHAR(50)        NOT NULL,
    [TableName]         NVARCHAR(100)       NOT NULL,
    [RecordID]          NVARCHAR(100)       NOT NULL,
    [OldValueJSON]      NVARCHAR(MAX)       NULL,
    [NewValueJSON]      NVARCHAR(MAX)       NULL,
    [IpAddress]         NVARCHAR(50)        NULL,
    [UserAgent]         NVARCHAR(500)       NULL,

    CONSTRAINT [PK_AuditLogs] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);
GO

CREATE CLUSTERED INDEX [IX_AuditLogs_Timestamp] ON [dbo].[AuditLogs] ([Timestamp] ASC);
GO

CREATE TABLE [dbo].[OutboxMessages] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [EventType]     NVARCHAR(200)       NOT NULL,
    [Payload]       NVARCHAR(MAX)       NOT NULL,
    [OccurredOn]    DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ProcessedAt]   DATETIME2(7)        NULL,
    [Error]         NVARCHAR(MAX)       NULL,
    [RetryCount]    INT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_OutboxMessages] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[HRVerificationQueue] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [UserId]            UNIQUEIDENTIFIER    NOT NULL,
    [EmployeeID]        NVARCHAR(50)        NOT NULL,
    [NationalID]        NVARCHAR(50)        NOT NULL,
    [FullName]          NVARCHAR(200)       NOT NULL,
    [SubmittedAt]       DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [VerifiedAt]        DATETIME2(7)        NULL,
    [VerifiedById]      UNIQUEIDENTIFIER    NULL,
    [Status]            NVARCHAR(50)        NOT NULL DEFAULT N'Pending',
    [RejectionReason]   NVARCHAR(500)       NULL,
    [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]        DATETIME2(7)        NULL,

    CONSTRAINT [PK_HRVerificationQueue] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_HRVerificationQueue_User] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HRVerificationQueue_Verifier] FOREIGN KEY ([VerifiedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CHK_HRVerificationQueue_Status] CHECK ([Status] IN (N'Pending', N'Approved', N'Rejected'))
);
GO

CREATE TABLE [dbo].[PasswordResetTokens] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [UserId]        UNIQUEIDENTIFIER    NOT NULL,
    [TokenHash]     NVARCHAR(500)       NOT NULL,
    [ExpiresAt]     DATETIME2(7)        NOT NULL,
    [UsedAt]        DATETIME2(7)        NULL,
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PasswordResetTokens_AppUsers] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[EmailLogs] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [RecipientEmail]    NVARCHAR(200)   NOT NULL,
    [Subject]       NVARCHAR(500)       NOT NULL,
    [TemplateType]  NVARCHAR(100)       NULL,
    [SentAt]        DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [DeliveryStatus]    NVARCHAR(50)    NOT NULL DEFAULT N'Pending',
    [ErrorMessage]  NVARCHAR(MAX)       NULL,
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_EmailLogs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CHK_EmailLogs_DeliveryStatus] CHECK ([DeliveryStatus] IN (N'Pending', N'Sent', N'Failed', N'Bounced'))
);
GO

CREATE TABLE [dbo].[SystemConfigurations] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConfigKey]         NVARCHAR(100)       NOT NULL,
    [ConfigValue]       NVARCHAR(MAX)       NOT NULL,
    [Description]       NVARCHAR(500)       NULL,
    [ModifiedAt]        DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedById]      UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_SystemConfigurations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SystemConfigurations_Modifier] FOREIGN KEY ([ModifiedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_SystemConfigurations_Key] UNIQUE NONCLUSTERED ([ConfigKey])
);
GO

CREATE TABLE [dbo].[SystemConfigurationHistory] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConfigKey]     NVARCHAR(100)       NOT NULL,
    [OldValue]      NVARCHAR(MAX)       NULL,
    [NewValue]      NVARCHAR(MAX)       NOT NULL,
    [ModifiedAt]    DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedById]  UNIQUEIDENTIFIER    NULL,
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_SystemConfigurationHistory] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SystemConfigurationHistory_Modifier] FOREIGN KEY ([ModifiedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[ResearchAttachments] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ResearchId]        UNIQUEIDENTIFIER    NOT NULL,
    [FileName]          NVARCHAR(255)       NOT NULL,
    [FilePath]          NVARCHAR(500)       NOT NULL,
    [FileSize]          BIGINT              NOT NULL,
    [ContentType]       NVARCHAR(100)       NOT NULL,
    [Sha256Hash]        NVARCHAR(64)        NOT NULL,
    [VersionNumber]     INT                 NOT NULL DEFAULT 1,
    [IsLatestVersion]   BIT                 NOT NULL DEFAULT 1,
    [UploadedById]      UNIQUEIDENTIFIER    NOT NULL,
    [UploadedAt]        DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_ResearchAttachments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ResearchAttachments_Research] FOREIGN KEY ([ResearchId]) 
        REFERENCES [dbo].[ResearchPapers]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResearchAttachments_Uploader] FOREIGN KEY ([UploadedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[FileAccessTokens] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    [AttachmentId]  UNIQUEIDENTIFIER    NOT NULL,
    [TokenHash]     NVARCHAR(500)       NOT NULL,
    [ExpiresAt]     DATETIME2(7)        NOT NULL,
    [CreatedById]   UNIQUEIDENTIFIER    NOT NULL,
    [CreatedAt]     DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_FileAccessTokens] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_FileAccessTokens_Attachment] FOREIGN KEY ([AttachmentId]) 
        REFERENCES [dbo].[ResearchAttachments]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FileAccessTokens_CreatedBy] FOREIGN KEY ([CreatedById]) 
        REFERENCES [dbo].[AppUsers]([Id]) ON DELETE NO ACTION
);
GO

-- =================================================================
-- SECTION 9: PERFORMANCE INDEXES
-- =================================================================

-- AppUsers
CREATE NONCLUSTERED INDEX [IX_AppUsers_RoleId] ON [dbo].[AppUsers] ([RoleId] ASC);
CREATE NONCLUSTERED INDEX [IX_AppUsers_DepartmentId] ON [dbo].[AppUsers] ([DepartmentId] ASC);
CREATE NONCLUSTERED INDEX [IX_AppUsers_DirectorateId] ON [dbo].[AppUsers] ([DirectorateId] ASC);
CREATE NONCLUSTERED INDEX [IX_AppUsers_EvaluatorStatus] ON [dbo].[AppUsers] ([EvaluatorStatus] ASC) INCLUDE ([BirthDate], [FullName]);
CREATE NONCLUSTERED INDEX [IX_AppUsers_AccountStatus] ON [dbo].[AppUsers] ([AccountStatus] ASC);
CREATE NONCLUSTERED INDEX [IX_AppUsers_LastLoginAt] ON [dbo].[AppUsers] ([LastLoginAt] ASC) WHERE ([LastLoginAt] IS NOT NULL);
GO

-- EvaluatorSpecializations
CREATE NONCLUSTERED INDEX [IX_EvaluatorSpecializations_EvaluatorId] ON [dbo].[EvaluatorSpecializations] ([EvaluatorId] ASC);
CREATE NONCLUSTERED INDEX [IX_EvaluatorSpecializations_SpecializationId] ON [dbo].[EvaluatorSpecializations] ([SpecializationId] ASC);
GO

-- ResearchPapers
CREATE NONCLUSTERED INDEX [IX_ResearchPapers_ResearcherId] ON [dbo].[ResearchPapers] ([ResearcherId] ASC);
CREATE NONCLUSTERED INDEX [IX_ResearchPapers_State] ON [dbo].[ResearchPapers] ([State] ASC) INCLUDE ([Title], [FinalScore], [CreatedAt]);
CREATE NONCLUSTERED INDEX [IX_ResearchPapers_MeetingId] ON [dbo].[ResearchPapers] ([MeetingId] ASC);
CREATE NONCLUSTERED INDEX [IX_ResearchPapers_MeetingMinutesId] ON [dbo].[ResearchPapers] ([MeetingMinutesId] ASC);
CREATE NONCLUSTERED INDEX [IX_ResearchPapers_ReplacedResearchId] ON [dbo].[ResearchPapers] ([ReplacedResearchId] ASC);
CREATE NONCLUSTERED INDEX [IX_ResearchPapers_DepartmentId] ON [dbo].[ResearchPapers] ([DepartmentId] ASC);
CREATE NONCLUSTERED INDEX [IX_ResearchPapers_DirectorateId] ON [dbo].[ResearchPapers] ([DirectorateId] ASC);
CREATE NONCLUSTERED INDEX [IX_ResearchPapers_Triage] ON [dbo].[ResearchPapers] ([State], [DirectorateId], [DepartmentId], [CreatedAt]) INCLUDE ([Title], [ResearcherId], [FinalScore]);
GO

-- TriageMappings
CREATE NONCLUSTERED INDEX [IX_TriageMappings_ResearchId] ON [dbo].[TriageMappings] ([ResearchId] ASC);
CREATE NONCLUSTERED INDEX [IX_TriageMappings_MappedById] ON [dbo].[TriageMappings] ([MappedById] ASC);
CREATE NONCLUSTERED INDEX [IX_TriageMappings_IsFinalized] ON [dbo].[TriageMappings] ([IsFinalized] ASC) WHERE ([IsFinalized] = 0);
GO

-- EvaluatorAssignments
CREATE NONCLUSTERED INDEX [IX_EvaluatorAssignments_ResearchId] ON [dbo].[EvaluatorAssignments] ([ResearchId] ASC);
CREATE NONCLUSTERED INDEX [IX_EvaluatorAssignments_EvaluatorId] ON [dbo].[EvaluatorAssignments] ([EvaluatorId] ASC) INCLUDE ([Status], [DueDate], [SubmittedDate]);
CREATE NONCLUSTERED INDEX [IX_EvaluatorAssignments_DueDate] ON [dbo].[EvaluatorAssignments] ([DueDate] ASC) WHERE ([SubmittedDate] IS NULL);
CREATE NONCLUSTERED INDEX [IX_EvaluatorAssignments_SLA_Active] ON [dbo].[EvaluatorAssignments] ([EvaluatorId], [Status], [DueDate]) WHERE ([Status] = N'Active' AND [SubmittedDate] IS NULL);
GO

-- Evaluations & ChairmanScores
CREATE NONCLUSTERED INDEX [IX_Evaluations_AssignmentId] ON [dbo].[Evaluations] ([AssignmentId] ASC);
CREATE NONCLUSTERED INDEX [IX_ChairmanScores_ResearchId] ON [dbo].[ChairmanScores] ([ResearchId] ASC);
CREATE NONCLUSTERED INDEX [IX_ChairmanScores_MeetingMinutesId] ON [dbo].[ChairmanScores] ([MeetingMinutesId] ASC);
GO

-- MeetingRSVPs, Attendance, Votes
CREATE NONCLUSTERED INDEX [IX_MeetingRSVPs_MeetingId] ON [dbo].[MeetingRSVPs] ([MeetingId] ASC);
CREATE NONCLUSTERED INDEX [IX_MeetingRSVPs_MemberId] ON [dbo].[MeetingRSVPs] ([MemberId] ASC);
CREATE NONCLUSTERED INDEX [IX_MeetingAttendance_MeetingId] ON [dbo].[MeetingAttendance] ([MeetingId] ASC);
CREATE NONCLUSTERED INDEX [IX_Votes_MeetingId] ON [dbo].[Votes] ([MeetingId] ASC);
CREATE NONCLUSTERED INDEX [IX_Votes_ResearchId] ON [dbo].[Votes] ([ResearchId] ASC);
GO

-- ChatMessages
CREATE NONCLUSTERED INDEX [IX_ChatMessages_ChannelId] ON [dbo].[ChatMessages] ([ChannelId] ASC);
CREATE NONCLUSTERED INDEX [IX_ChatMessages_SenderId] ON [dbo].[ChatMessages] ([SenderId] ASC);
CREATE NONCLUSTERED INDEX [IX_ChatMessages_ReceiverId] ON [dbo].[ChatMessages] ([ReceiverId] ASC);
CREATE NONCLUSTERED INDEX [IX_ChatMessages_SentAt] ON [dbo].[ChatMessages] ([SentAt] ASC);
GO

-- Notifications & AuditLogs
CREATE NONCLUSTERED INDEX [IX_Notifications_UserId] ON [dbo].[Notifications] ([UserId] ASC) WHERE ([IsRead] = 0);
CREATE NONCLUSTERED INDEX [IX_Notifications_CreatedAt] ON [dbo].[Notifications] ([CreatedAt] ASC);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_TableName_RecordID] ON [dbo].[AuditLogs] ([TableName] ASC, [RecordID] ASC);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_OperatorEmployeeID] ON [dbo].[AuditLogs] ([OperatorEmployeeID] ASC);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_ActionType] ON [dbo].[AuditLogs] ([ActionType] ASC);
GO

-- OutboxMessages
CREATE NONCLUSTERED INDEX [IX_OutboxMessages_ProcessedAt] ON [dbo].[OutboxMessages] ([ProcessedAt] ASC) WHERE ([ProcessedAt] IS NULL);
CREATE NONCLUSTERED INDEX [IX_OutboxMessages_RetryCount] ON [dbo].[OutboxMessages] ([RetryCount] ASC);
GO

-- HRVerificationQueue, Attachments, FileAccessTokens
CREATE NONCLUSTERED INDEX [IX_HRVerificationQueue_Status] ON [dbo].[HRVerificationQueue] ([Status] ASC) WHERE ([Status] = N'Pending');
CREATE NONCLUSTERED INDEX [IX_ResearchAttachments_ResearchId] ON [dbo].[ResearchAttachments] ([ResearchId] ASC);
CREATE NONCLUSTERED INDEX [IX_ResearchAttachments_IsLatestVersion] ON [dbo].[ResearchAttachments] ([IsLatestVersion] ASC) WHERE ([IsLatestVersion] = 1);
CREATE NONCLUSTERED INDEX [IX_FileAccessTokens_ExpiresAt] ON [dbo].[FileAccessTokens] ([ExpiresAt] ASC);
GO

-- MinistryBatches & BatchItems
CREATE NONCLUSTERED INDEX [IX_MinistryBatches_Status] ON [dbo].[MinistryBatches] ([Status] ASC);
CREATE NONCLUSTERED INDEX [IX_BatchItems_BatchId] ON [dbo].[BatchItems] ([BatchId] ASC);
CREATE NONCLUSTERED INDEX [IX_BatchItems_ResearchId] ON [dbo].[BatchItems] ([ResearchId] ASC);
GO

-- =================================================================
-- SECTION 10: FULL-TEXT SEARCH CATALOG
-- =================================================================

CREATE FULLTEXT CATALOG [BOC_Research_Catalog] AS DEFAULT;
GO

CREATE FULLTEXT INDEX ON [dbo].[ResearchPapers]([Title], [Abstract])
    KEY INDEX [PK_ResearchPapers]
    ON [BOC_Research_Catalog]
    WITH STOPLIST = SYSTEM;
GO

-- =================================================================
-- SECTION 11: VIEWS
-- =================================================================

CREATE VIEW [dbo].[vw_EligibleEvaluators] AS
SELECT 
    r.[Id] AS ResearchId,
    u.[Id] AS EvaluatorId,
    u.[FullName] AS EvaluatorName,
    u.[DepartmentId] AS EvaluatorDepartmentId,
    u.[DirectorateId] AS EvaluatorDirectorateId,
    s.[Id] AS SpecializationId,
    s.[Name] AS SpecializationName,
    (SELECT COUNT(*) FROM [dbo].[EvaluatorAssignments] ea WHERE ea.[EvaluatorId] = u.[Id]) AS TotalAssignedCount,
    (SELECT COUNT(*) FROM [dbo].[EvaluatorAssignments] ea2 WHERE ea2.[EvaluatorId] = u.[Id] AND ea2.[Status] = N'Active' AND ea2.[SubmittedDate] IS NULL) AS ActiveUnderEvaluationCount,
    (SELECT MAX(ea3.[AssignedDate]) FROM [dbo].[EvaluatorAssignments] ea3 WHERE ea3.[EvaluatorId] = u.[Id]) AS LastAssignedDate
FROM [dbo].[ResearchPapers] r
CROSS JOIN [dbo].[AppUsers] u
INNER JOIN [dbo].[AppRoles] rl ON u.[RoleId] = rl.[Id]
LEFT JOIN [dbo].[EvaluatorSpecializations] es ON u.[Id] = es.[EvaluatorId]
LEFT JOIN [dbo].[Specializations] s ON es.[SpecializationId] = s.[Id]
WHERE 
    rl.[Name] IN (N'Committee Members', N'External Evaluators')
    AND u.[EvaluatorStatus] = N'Active'
    AND u.[AccountStatus] = N'Active'
    AND u.[DepartmentId] <> r.[DepartmentId]
    AND u.[DirectorateId] <> r.[DirectorateId]
    AND (r.[CategoryId] IS NULL OR s.[Id] IS NULL OR s.[Id] IN (
        SELECT [SpecializationId] FROM [dbo].[ResearchCategories] WHERE [Id] = r.[CategoryId]
    ));
GO

CREATE VIEW [dbo].[vw_SLABreachReport] AS
SELECT 
    ea.[Id] AS AssignmentId,
    ea.[ResearchId],
    ea.[EvaluatorId],
    u.[FullName] AS EvaluatorName,
    u.[Email] AS EvaluatorEmail,
    r.[Title] AS ResearchTitle,
    r.[TrackingNumber],
    ea.[AssignedDate],
    ea.[DueDate],
    ea.[SubmittedDate],
    ea.[ReminderCount],
    DATEDIFF(DAY, ea.[AssignedDate], GETUTCDATE()) AS DaysElapsed,
    CASE 
        WHEN ea.[SubmittedDate] IS NULL AND GETUTCDATE() > ea.[DueDate] THEN N'Breached'
        WHEN ea.[SubmittedDate] IS NULL AND DATEDIFF(DAY, ea.[AssignedDate], GETUTCDATE()) >= 10 THEN N'Warning'
        ELSE N'OnTrack'
    END AS SLABreachStatus
FROM [dbo].[EvaluatorAssignments] ea
INNER JOIN [dbo].[AppUsers] u ON ea.[EvaluatorId] = u.[Id]
INNER JOIN [dbo].[ResearchPapers] r ON ea.[ResearchId] = r.[Id]
WHERE ea.[Status] = N'Active';
GO

CREATE VIEW [dbo].[vw_RetirementEligibleEvaluators] AS
SELECT 
    u.[Id] AS EvaluatorId,
    u.[FullName],
    u.[BirthDate],
    u.[EvaluatorStatus],
    DATEDIFF(YEAR, u.[BirthDate], GETUTCDATE()) AS CurrentAge,
    CASE 
        WHEN DATEDIFF(YEAR, u.[BirthDate], GETUTCDATE()) >= 63 THEN N'Retirement_Immediate'
        WHEN DATEDIFF(YEAR, u.[BirthDate], DATEADD(YEAR, 1, GETUTCDATE())) >= 63 THEN N'Retirement_Upcoming'
        ELSE N'Active'
    END AS RetirementFlag
FROM [dbo].[AppUsers] u
INNER JOIN [dbo].[AppRoles] rl ON u.[RoleId] = rl.[Id]
WHERE 
    rl.[Name] IN (N'Committee Members', N'External Evaluators')
    AND u.[BirthDate] IS NOT NULL;
GO

CREATE VIEW [dbo].[vw_EvaluatorWorkloadSummary] AS
SELECT 
    u.[Id] AS EvaluatorId,
    u.[FullName],
    u.[Email],
    COUNT(ea_all.[Id]) AS TotalAssignedCount,
    SUM(CASE WHEN ea_active.[Status] = N'Active' AND ea_active.[SubmittedDate] IS NULL THEN 1 ELSE 0 END) AS ActiveUnderEvaluationCount,
    MAX(ea_all.[AssignedDate]) AS LastAssignedDate,
    CASE 
        WHEN COUNT(ea_all.[Id]) = 0 THEN N'Tier_1_NeverAssigned'
        WHEN SUM(CASE WHEN ea_active.[Status] = N'Active' AND ea_active.[SubmittedDate] IS NULL THEN 1 ELSE 0 END) = 0 THEN N'Tier_2_Idle'
        ELSE N'Tier_3_Active'
    END AS WorkloadTier
FROM [dbo].[AppUsers] u
INNER JOIN [dbo].[AppRoles] rl ON u.[RoleId] = rl.[Id]
LEFT JOIN [dbo].[EvaluatorAssignments] ea_all ON u.[Id] = ea_all.[EvaluatorId]
LEFT JOIN [dbo].[EvaluatorAssignments] ea_active ON u.[Id] = ea_active.[EvaluatorId]
WHERE rl.[Name] IN (N'Committee Members', N'External Evaluators')
    AND u.[EvaluatorStatus] = N'Active'
    AND u.[AccountStatus] = N'Active'
GROUP BY u.[Id], u.[FullName], u.[Email];
GO

-- =================================================================
-- SECTION 12: SEED DATA
-- =================================================================

INSERT INTO [dbo].[AppRoles] ([Id], [Name], [NormalizedName], [Description], [IsActive])
VALUES
    (NEWID(), N'Admin', N'ADMIN', N'System Administrator with full access and omnipotent override.', 1),
    (NEWID(), N'Committee Chairman', N'COMMITTEE CHAIRMAN', N'Committee Chairman with grading, freeze, and override authority.', 1),
    (NEWID(), N'Deputy Chairman', N'DEPUTY CHAIRMAN', N'Deputy Chairman with delegated triage and self-grading authority.', 1),
    (NEWID(), N'Committee Secretary', N'COMMITTEE SECRETARY', N'Secretary managing workflow, SLA, and meeting scheduling.', 1),
    (NEWID(), N'Supervisor', N'SUPERVISOR', N'Elevated Committee Member with meeting management and triage authority.', 1),
    (NEWID(), N'Data Entry Clerk', N'DATA ENTRY CLERK', N'Restricted CRUD on metadata fields only.', 1),
    (NEWID(), N'Committee Members', N'COMMITTEE MEMBERS', N'Committee members with evaluation and voting rights.', 1),
    (NEWID(), N'External Evaluators', N'EXTERNAL EVALUATORS', N'External evaluators with scoring rights.', 1),
    (NEWID(), N'Researchers', N'RESEARCHERS', N'Research paper submitters.', 1);
GO

INSERT INTO [dbo].[SystemConfigurations] ([Id], [ConfigKey], [ConfigValue], [Description])
VALUES
    (NEWID(), N'SLA_DefaultDays', N'14', N'Default SLA duration in days for evaluator assignments.'),
    (NEWID(), N'SLA_WarningDays', N'10', N'Days elapsed before SLA breach warning is triggered.'),
    (NEWID(), N'Retirement_Age_Cap', N'63', N'Legal retirement age cap for auto-evaluator suspension.'),
    (NEWID(), N'Plagiarism_Lockout_Months', N'3', N'Months to lock research on plagiarism detection.'),
    (NEWID(), N'Minutes_Freeze_Enabled', N'true', N'Global toggle for immutable minutes freezing.'),
    (NEWID(), N'File_MaxSizeMB', N'50', N'Maximum file upload size in megabytes.'),
    (NEWID(), N'File_AccessTokenExpiryHours', N'24', N'Hours before file access token expires.'),
    (NEWID(), N'JWT_AccessTokenExpiryMinutes', N'15', N'Minutes before JWT access token expires.'),
    (NEWID(), N'JWT_RefreshTokenExpiryDays', N'7', N'Days before refresh token expires.'),
    (NEWID(), N'RateLimit_LoginAttempts', N'5', N'Max login attempts per 15 minutes.'),
    (NEWID(), N'RateLimit_APICallsPerMinute', N'100', N'Max API calls per minute per user.'),
    (NEWID(), N'Outbox_RetryMaxCount', N'5', N'Maximum retries for outbox message delivery.'),
    (NEWID(), N'Outbox_DispatchIntervalSeconds', N'30', N'Seconds between outbox dispatcher polls.'),
    (NEWID(), N'DataRetention_AuditLogsYears', N'7', N'Years to retain audit logs.'),
    (NEWID(), N'DataRetention_ArchivedResearchYears', N'10', N'Years to retain archived research.'),
    (NEWID(), N'PasswordReset_TokenExpiryMinutes', N'15', N'Minutes before password reset token expires.');
GO

-- =================================================================
-- SECTION 13: PARTITION FUNCTION FOR AUDIT LOGS (Yearly)
-- =================================================================

CREATE PARTITION FUNCTION [PF_AuditLogs_Yearly] (DATETIME2(7))
AS RANGE RIGHT FOR VALUES 
    ('2024-01-01', '2025-01-01', '2026-01-01', '2027-01-01', '2028-01-01', 
     '2029-01-01', '2030-01-01', '2031-01-01', '2032-01-01', '2033-01-01');
GO

CREATE PARTITION SCHEME [PS_AuditLogs_Yearly]
AS PARTITION [PF_AuditLogs_Yearly]
ALL TO ([PRIMARY]);
GO

PRINT N'>>> BOC Research Evaluation Database Phase A deployed successfully.';
PRINT N'>>> Tables: 35 | Views: 4 | Indexes: 40+ | Full-Text: 1 Catalog';
PRINT N'>>> Architecture: Guid PKs | Restrict Delete | RowVersion | Computed Columns | Partition-Ready';
GO