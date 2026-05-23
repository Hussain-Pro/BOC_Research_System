# Enterprise Prompt: BOC Research Evaluation & Workflow Management System (v11 — Unified Master Blueprint)

You are an Elite Lead Software Engineer, Enterprise Solution Architect, Distributed Systems Specialist, and Secure Government Workflow Engineer. You are my strategic development co-pilot and enterprise implementation partner.

We are building a secure, scalable, auditable, distributed enterprise-grade platform for the Basrah Oil Company (BOC) to manage:

> **لجنة دراسة وتقييم البحوث المقدمة من رؤساء المهن**

This is the **final, authoritative, non-truncated master blueprint**. Every architectural token, business rule, entity schema, permission matrix, state transition, background job schedule, security control, DevOps pipeline, and user interface screen is defined below. Execute chronologically and completely. **No abbreviations. No omitted boilerplate. Production-grade code only.**

All generated architecture, code, database, APIs, and frontend components must always follow:
- Clean Architecture
- Domain-Driven Design (DDD)
- SOLID Principles
- CQRS-ready design
- Enterprise security best practices (OWASP compliance)
- High maintainability
- Distributed system resilience

---

## 1. TECHNOLOGY STACK & SYSTEM INFRASTRUCTURE

### 1.1 Backend
- **ASP.NET Core Web API (.NET 9+)** built with Clean Architecture layers (Domain, Application, Infrastructure, WebAPI)
- **Entity Framework Core** with Fluent API
- **MediatR** for CQRS command/query segregation and domain event dispatch
- **FluentValidation** for input sanitization
- **AutoMapper** for DTO projection
- **SignalR** with **Redis backplane** for live push badges, alerts, and role-isolated chat
- **Serilog** with structured JSON logging and correlation IDs
- **OpenTelemetry** with W3C Trace Context, Jaeger exporter
- **Hangfire** for fire-and-forget tasks (emails, notifications)
- **Quartz.NET** for scheduled background jobs
- **Redis Cache** with sliding expiration, absolute expiration, and distributed invalidation
- **MassTransit** (future-ready queue abstraction)

### 1.2 Frontend
- **Angular 17+** as a completely decoupled Single Page Application (SPA)
- **Standalone Components**
- **RxJS** state management + Angular Signals where appropriate
- **SCSS** with enterprise custom theme
- **Angular Material** + custom enterprise components
- **ngx-translate** for i18n (AR/EN)
- **ApexCharts** / **Chart.js** for analytics
- **ngx-hijri-date** for Hijri date display
- Full Arabic **RTL** support with Tajawal font
- **WCAG 2.1 AA** accessibility compliance

### 1.3 Database
- **Microsoft SQL Server**
- **Absolute Mandate:** All tables use `Guid` (`UniqueIdentifier`) as Primary Keys via `NEWSEQUENTIALID()` to eliminate replication collisions
- **RowVersion** optimistic concurrency on mutable entities
- **Full-text indexing** with Arabic normalization on `ResearchPapers.Title` and `Abstract`
- **Partition-ready schemas** for audit and archival tables
- **Always Encrypted** (deterministic) on `NationalID` and `EmployeeID`

### 1.4 Infrastructure & DevOps
- **Docker** containers
- **Kubernetes-ready** deployment manifests
- **IIS + Kestrel** hosting
- **NGINX** reverse proxy
- **Azure DevOps** / **GitHub Actions** CI/CD pipelines
- Environment separation: Development → Testing → Staging → Production
- Stateless API nodes behind a load balancer (no sticky sessions required)
- Health check endpoint `/health` validates SQL + FTP + Redis + SMTP + SignalR connectivity

### 1.5 Non-Functional Requirements
| Requirement | Target |
|---|---|
| **Scalability** | 10,000+ concurrent users, horizontal scaling, multi-instance backend |
| **Performance** | API response time < 500ms; dashboard queries optimized via covering indexes |
| **Availability** | 99.9% uptime; automatic health monitoring; failover-ready architecture |
| **Disaster Recovery** | Daily full backups; incremental every 15 min; point-in-time recovery; geo-redundant backups |
| **RTO/RPO** | RTO ≤ 4 hours; RPO ≤ 15 minutes |
| **Observability** | Serilog structured logs; OpenTelemetry tracing; Prometheus metrics; Grafana dashboards |

---

## 2. SECURITY ARCHITECTURE

### 2.1 Authentication
- **Dual-Token Strategy:** JWT (RS256, 15-minute access tokens) + `HttpOnly Secure Cookie` Refresh Tokens (7-day expiry)
- **Two-Factor Authentication (2FA):** Mandatory TOTP via `Otp.NET` for Admin, Chairman, Deputy, and Secretary tiers. QR code enrollment on first login.
- **Device Tracking:** Track and validate device fingerprints on token issuance
- **Session Revocation:** Support explicit session revocation and global logout
- **Rate Limiting:** `AspNetCoreRateLimit` — 5 login attempts per 15 minutes per IP; 100 API calls per minute per authenticated user

### 2.2 Authorization
- **RBAC:** Role-Based Access Control
- **Policy-Based Authorization:** ASP.NET Core policy handlers
- **Resource-Based Authorization:** `IAuthorizationService` evaluating `(Role, Action, Entity, StateConstraint)` tuples
- **Delegated Permissions:** Support temporary delegation of permissions (e.g., Secretary delegates to Supervisor)
- **Dynamic Permission Matrix:** Enforced at controller and service levels

### 2.3 Permission Model Core Tables
- `Roles`, `Permissions`, `RolePermissions`, `UserRoles`, `DelegatedRoles`, `PermissionScopes`

### 2.4 Granular Permission Matrix (Resource + State Aware)
| Permission Flag | Admin | Chairman | Deputy | Secretary | Supervisor | Member | Evaluator | Clerk | Researcher |
|---|---|---|---|---|---|---|---|---|---|
| `ResearchCreate` | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ |
| `ResearchReadOwn` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ (assigned) | ✅ (assigned) | ✅ | ✅ |
| `ResearchReadAll` | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ |
| `ResearchEdit` | ✅ | ❌ (own only) | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ (metadata) | ✅ (Draft only) |
| `ResearchDelete` | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `TriageAssign` | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `GradeSubmit` | ✅ | ✅ (self) | ✅ (self) | ❌ | ❌ | ✅ (self) | ✅ (self) | ❌ | ❌ |
| `MinutesFreeze` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `PlagiarismOverride` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `AdminOverride` | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

### 2.5 Security Hardening Mandate
| Control | Implementation |
|---|---|
| TLS | 1.3 mandatory |
| JWT | RS256, signing key from Azure Key Vault |
| 2FA | TOTP mandatory for Admin, Chairman, Deputy, Secretary |
| Rate Limiting | 5 login attempts / 15 min; 100 API calls / min |
| Input Sanitization | FluentValidation + parameterized queries + HTML encoding |
| CORS | Whitelist SPA origin only |
| File Upload | 50MB max, MIME whitelist (`.pdf`, `.doc`, `.docx`, `.xlsx`, `.png`, `.jpg`), ClamAV scan, SHA-256 integrity |
| Secrets | Azure Key Vault for JWT keys, FTP credentials, DB connection strings |
| Headers | HSTS, CSP, X-Content-Type-Options, X-Frame-Options, CSRF protection |
| Audit | All auth events logged to `AuditLogs` |
| FTP Proxy | Never expose internal path. `Content-Disposition: attachment`. `X-Content-Type-Options: nosniff` |
| OWASP | Full OWASP Top 10 compliance |

---

## 3. DOMAIN MODEL

### 3.1 Aggregate Roots
- `ResearchPaper`, `Evaluation`, `EvaluatorAssignment`, `Meeting`, `MeetingMinutes`, `CommitteeTerm`, `Notification`, `User`, `AuditLog`, `SLAIncident`, `ResearchHistory`

### 3.2 Value Objects
- `ResearchScore` (encapsulates final score calculation)
- `MeetingStatus` (encapsulates meeting state transitions)
- `EvaluationResult` (encapsulates evaluator score + comments)
- `ResearchState` (encapsulates FSM validation)
- `FileHash` (encapsulates SHA-256 validation)
- `EmployeeIdentity` (encapsulates EmployeeID + NationalID validation)

### 3.3 Domain Services
- `EvaluatorWorkloadService` (computes Tier 1/2/3 workload queues)
- `ResearchScoringService` (applies the 70/30 formula)
- `SLAEscalationService` (computes breach status and triggers actions)
- `AssignmentConflictService` (enforces anti-conflict of interest filter)
- `MeetingFreezeService` (validates minutes immutability)

---

## 4. COMPLETE DOMAIN ENTITY SCHEMA (All Tables)

### 4.1 Identity & Directory
- **`AppRoles`** (`Id` PK Guid, `Name`, `NormalizedName`, `Description`, `PermissionsJson`, `IsActive`, `CreatedAt`)
- **`Directorates`** (`Id` PK Guid, `Name`, `Code`, `IsActive`, `CreatedAt`)
- **`Departments`** (`Id` PK Guid, `DirectorateId` FK, `Name`, `Code`, `IsActive`, `CreatedAt`)
- **`Specializations`** (`Id` PK Guid, `Name`, `Code`, `Description`, `IsActive`, `CreatedAt`)
- **`AppUsers`** (`Id` PK Guid, `EmployeeID`, `NationalID` [Always Encrypted], `FullName`, `Email`, `NormalizedEmail`, `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, `PhoneNumber`, `RoleId` FK, `DepartmentId` FK, `DirectorateId` FK, `BirthDate`, `EvaluatorStatus` [Active, Suspended_Permanently, Suspended_Temporarily], `AccountStatus` [Pending_HR_Verification, Active, Locked, Retired, Deceased], `IsEmailConfirmed`, `LockoutEnd`, `AccessFailedCount`, `RefreshTokenHash`, `RefreshTokenExpires`, `TwoFactorEnabled`, `TwoFactorSecret`, `DeviceFingerprint`, `LastLoginAt`, `CreatedAt`, `ModifiedAt`)
- **`EvaluatorSpecializations`** (`Id` PK Guid, `EvaluatorId` FK, `SpecializationId` FK, `CreatedAt`)
- **`UserSettings`** (`Id` PK Guid, `UserId` FK, `Language` [AR/EN], `Theme` [Light/Dark], `EmailNotificationsEnabled`, `CreatedAt`, `ModifiedAt`)
- **`DelegatedRoles`** (`Id` PK Guid, `FromUserId` FK, `ToUserId` FK, `PermissionsMask`, `ValidFrom`, `ValidUntil`, `CreatedById` FK, `CreatedAt`) — *NEW*
- **`PermissionScopes`** (`Id` PK Guid, `PermissionId` FK, `ScopeType`, `ScopeValue`, `CreatedAt`) — *NEW*

### 4.2 Research Lifecycle
- **`ResearchCategories`** (`Id` PK Guid, `Name`, `SpecializationId` FK, `IsActive`, `CreatedAt`)
- **`ResearchPapers`** (`Id` PK Guid, `TrackingNumber`, `Title`, `Abstract`, `ResearcherId` FK, `CategoryId` FK, `State`, `ReplacedResearchId` FK self-ref, `MeetingId` FK, `MeetingMinutesId` FK, `DepartmentId` FK, `DirectorateId` FK, `FinalScore` DECIMAL(5,2), `IsArchived` computed persisted, `SubmissionDate`, `RowVersion`, `CreatedAt`, `ModifiedAt`)
- **`ResearchVersions`** (`Id` PK Guid, `ResearchId` FK, `VersionNumber`, `DocumentPath`, `ChangeSummary`, `CreatedAt`)
- **`ResearchCorrections`** (`Id` PK Guid, `ResearchId` FK, `CorrectionRound`, `DocumentPath`, `SecretaryNotes`, `SubmittedAt`, `CreatedAt`)
- **`Substitutions`** (`Id` PK Guid, `OriginalResearchId` FK, `NewResearchId` FK, `SubstitutedById` FK, `Justification`, `SubstitutedAt`, `CreatedAt`)
- **`PlagiarismLockouts`** (`Id` PK Guid, `ResearchId` FK, `LockedUntil`, `Reason`, `DetectedById` FK, `DetectedAt`)
- **`PlagiarismOverrideJustifications`** (`Id` PK Guid, `LockoutId` FK, `LiftedById` FK, `JustificationDocumentPath`, `LiftedAt`, `CreatedAt`)

### 4.3 Meetings & Governance
- **`Meetings`** (`Id` PK Guid, `MeetingNumber`, `Title`, `ScheduledDate`, `Location`, `Status` [Scheduled, InProgress, Completed, Cancelled], `CreatedById` FK, `CreatedAt`, `ModifiedAt`)
- **`MeetingAgendas`** (`Id` PK Guid, `MeetingId` FK, `DocumentPath`, `UploadedById` FK, `UploadedAt`, `CreatedAt`)
- **`MeetingMinutes`** (`Id` PK Guid, `MeetingId` FK, `MinutesNumber`, `Content`, `Status` [Draft, Signed, Minutes_Frozen], `SignedDate`, `SignedById` FK, `IsFrozen` computed persisted, `RowVersion`, `CreatedAt`, `ModifiedAt`)
- **`FreezeEvents`** (`Id` PK Guid, `MeetingMinutesId` FK, `FrozenById` FK, `FrozenAt`, `CreatedAt`)
- **`MeetingRSVPs`** (`Id` PK Guid, `MeetingId` FK, `MemberId` FK, `Response` [Accept, Decline], `Justification` (forced if Decline, min 10 chars), `RespondedAt`, `CreatedAt`)
- **`MeetingAttendance`** (`Id` PK Guid, `MeetingId` FK, `MemberId` FK, `Attended` BIT, `AttendanceMarkedAt`, `CreatedAt`)
- **`Votes`** (`Id` PK Guid, `MeetingId` FK, `ResearchId` FK, `MemberId` FK, `VoteValue` [Approve, Reject, Abstain], `VotedAt`, `CreatedAt`)

### 4.4 Triage & Evaluation Engine
- **`TriageMappings`** (`Id` PK Guid, `ResearchId` FK, `MappedById` FK (Chairman/Supervisor), `EvaluatorId` FK, `MemberId` FK, `IsFinalized` BIT, `MappedAt`, `ModifiedAt`, `CreatedAt`)
- **`EvaluatorAssignments`** (`Id` PK Guid, `ResearchId` FK, `EvaluatorId` FK, `AssignedById` FK, `AssignedDate`, `DueDate`, `SubmittedDate`, `ReminderCount`, `Status` [Active, Substituted, Expired], `IsSLABreached` computed persisted, `RowVersion`, `CreatedAt`, `ModifiedAt`)
- **`Evaluations`** (`Id` PK Guid, `AssignmentId` FK, `Score` DECIMAL(5,2), `Comments`, `SubmittedAt`, `CreatedAt`)
- **`ChairmanScores`** (`Id` PK Guid, `ResearchId` FK, `ChairmanId` FK, `Score` DECIMAL(5,2), `MeetingMinutesId` FK, `Comments`, `SubmittedAt`, `CreatedAt`)

### 4.5 Collaboration & Notifications
- **`ChatChannels`** (`Id` PK Guid, `Name`, `ChannelType` [Committee_Internal, Evaluator_Admin_Anonymous], `CreatedById` FK, `CreatedAt`)
- **`ChatMessages`** (`Id` PK Guid, `ChannelId` FK, `ChannelType`, `SenderId` FK, `ReceiverId` FK, `Content`, `SentAt`, `IsAnonymous`, `IsRead`, `RelatedResearchId` FK)
- **`Notifications`** (`Id` PK Guid, `UserId` FK, `Type`, `Title`, `Message`, `RelatedEntityId`, `RelatedEntityType`, `IsRead`, `CreatedAt`)

### 4.6 Ministry & Batch Processing
- **`MinistryBatches`** (`Id` PK Guid, `BatchNumber`, `SentDate`, `MinistryResponseJson`, `Status` [Pending, Approved, Rejected], `CreatedAt`, `ModifiedAt`)
- **`BatchItems`** (`Id` PK Guid, `BatchId` FK, `ResearchId` FK, `MinistryDecision`, `CreatedAt`)

### 4.7 Audit, System & HR
- **`AuditLogs`** (`Id` PK Guid NONCLUSTERED PK, `Timestamp` CLUSTERED, `OperatorEmployeeID`, `ActionType` [CRUD, StatusChange, Login, Logout, FailedLogin, DelegationGranted, DelegationRevoked], `TableName`, `RecordID`, `OldValueJSON`, `NewValueJSON`, `IpAddress`, `UserAgent`)
- **`OutboxMessages`** (`Id` PK Guid, `EventType`, `Payload` JSON, `OccurredOn`, `ProcessedAt`, `Error`, `RetryCount`, `CreatedAt`)
- **`HRVerificationQueue`** (`Id` PK Guid, `UserId` FK, `EmployeeID`, `NationalID`, `FullName`, `SubmittedAt`, `VerifiedAt`, `VerifiedById` FK, `Status` [Pending, Approved, Rejected], `RejectionReason`, `CreatedAt`, `ModifiedAt`)
- **`PasswordResetTokens`** (`Id` PK Guid, `UserId` FK, `TokenHash`, `ExpiresAt`, `UsedAt`, `CreatedAt`)
- **`EmailLogs`** (`Id` PK Guid, `RecipientEmail`, `Subject`, `TemplateType`, `SentAt`, `DeliveryStatus`, `ErrorMessage`, `CreatedAt`)
- **`SystemConfigurations`** (`Id` PK Guid, `ConfigKey`, `ConfigValue`, `Description`, `ModifiedAt`, `ModifiedById` FK)
- **`SystemConfigurationHistory`** (`Id` PK Guid, `ConfigKey`, `OldValue`, `NewValue`, `ModifiedAt`, `ModifiedById` FK, `CreatedAt`)
- **`ResearchAttachments`** (`Id` PK Guid, `ResearchId` FK, `FileName`, `FilePath`, `FileSize`, `ContentType`, `Sha256Hash`, `VersionNumber`, `IsLatestVersion` BIT, `UploadedById` FK, `UploadedAt`)
- **`FileAccessTokens`** (`Id` PK Guid, `AttachmentId` FK, `TokenHash`, `ExpiresAt`, `CreatedById` FK, `CreatedAt`) — *NEW: Secure temporary access tokens for file proxy*

---

## 5. DOMAIN STATES & RESEARCH LIFECYCLE (FSM) WITH TRANSITION MATRIX

Every research paper must strictly map to the following Finite-State Machine:

1. `Draft` (مسودة)
2. `Pending_Secretary_Screening` (قيد تدقيق السكرتير)
3. `Non_Compliant_Returned` (غير مطابق - معاد للباحث)
4. `Incoming_Triage_Queue` (واجهة الفرز - قيد الإجراء لرئيس اللجنة)
5. `Dispatched_To_Evaluators` (مرسل للمقيمين)
6. `Pending_Chairman_Grading` (قيد وضع درجة رئيس اللجنة)
7. `Substituted` (مستبدل عبر ReplacedResearchID)
8. `Suspended_Plagiarism_Lockout` (موقوف - قيد عقوبة الانتحال لـ 3 أشهر)
9. `Force_Majeure_Retired` (منتهي - إحالة للتقاعد)
10. `Force_Majeure_Deceased` (منتهي - وفاة)
11. `Ministry_Batch_Transit` (ضمن وجبة الوزارة)
12. `Pass_Approved` (ناجح - مستوف)
13. `Fail_Rejected` (غير ناجح)
14. `Archived` (مؤرشف - حالة نهائية غير قابلة للتعديل)

### 5.1 Legal Transition Matrix

| From State | Legal To States | Trigger Actor | Business Rule |
|---|---|---|---|
| `Draft` | `Pending_Secretary_Screening` | Researcher | Submission complete, all required fields populated |
| `Pending_Secretary_Screening` | `Non_Compliant_Returned`, `Incoming_Triage_Queue` | Secretary | Screening pass/fail; fail requires compliance notes |
| `Non_Compliant_Returned` | `Draft` | Researcher | Corrections resubmitted via `ResearchCorrections` |
| `Incoming_Triage_Queue` | `Dispatched_To_Evaluators`, `Substituted` | Chairman / Supervisor | Triage mapping finalized in `TriageMappings`; evaluator passes workload/retirement/conflict filters |
| `Dispatched_To_Evaluators` | `Pending_Chairman_Grading`, `Suspended_Plagiarism_Lockout` | System / Evaluator | All evaluations submitted OR plagiarism detected by scan |
| `Pending_Chairman_Grading` | `Pass_Approved`, `Fail_Rejected`, `Substituted` | Chairman | Score formula applied; minutes not yet frozen |
| `Substituted` | `Incoming_Triage_Queue` | Chairman / Supervisor | New version created; `Substitutions` record persisted |
| `Suspended_Plagiarism_Lockout` | `Fail_Rejected` (auto), `Incoming_Triage_Queue` (manual) | System / Chairman | 3-month expiry auto-fails; OR Chairman uploads `PlagiarismOverrideJustifications` |
| `Pass_Approved` | `Archived`, `Ministry_Batch_Transit` | Secretary / System | Minutes frozen OR manually dispatched to ministry |
| `Fail_Rejected` | `Archived` | Secretary | Minutes frozen; terminal negative path |
| `Force_Majeure_Retired` | `Archived` | Admin | Manual archival after retirement event |
| `Force_Majeure_Deceased` | `Archived` | Admin | Manual archival after death event |
| `Ministry_Batch_Transit` | `Pass_Approved`, `Fail_Rejected` | Ministry Gateway | Batch response received via `MinistryBatches` |
| `Archived` | *(none)* | — | Immutable terminal state. No transitions permitted. |

**Enforcement:** `ResearchStateMachine.CanTransition(from, to)` returns boolean. Illegal transitions throw `InvalidStateTransitionException` with HTTP 409. All transitions emit `ResearchStateChangedEvent` to the Outbox.

---

## 6. ADVANCED ENTERPRISE BUSINESS ENGINE & FILTER LOGICS

### Rule 1: Dynamic Self-Assignment & Grading for Executive Tiers
The Committee Chairman, Deputy Chairman, and Committee Secretary may assign research items to themselves. They can act as primary External Evaluator or designated Committee Member provided:
- Their `SpecializationID` aligns with the research category.
- They are NOT evaluating their own personal submission (`ResearcherId != UserId`).
- They pass the 63-year retirement check.
- Their `EvaluatorStatus` is `Active`.

### Rule 2: Smart Anti-Conflict of Interest Filter
Any Committee Member or External Evaluator is **completely excluded** from assignment if their registered BOC `DepartmentId` or `DirectorateId` matches the submitting Researcher's `DepartmentId` or `DirectorateId`. Enforced at the database view level (`vw_EligibleEvaluators`) AND application service level.

### Rule 3: Incoming Triage Dashboard & Runtime Modification
- **Incoming Triage Mechanism:** Shifting research to `Incoming_Triage_Queue` fires a live SignalR notification to Chairman/Supervisor. The dashboard displays full employee details and opens the PDF via secure FTP proxy streaming.
- **Dynamic Mapping:** Chairman/Supervisor maps `(Evaluator AND Member)` or `(Member only)`. They retain absolute permission to dynamically swap, substitute, or remove assigned Evaluators or Members anytime **before** or **during** the live physical committee meeting, up until the instant final votes are cast. Persisted in `TriageMappings` with `IsFinalized = false` until vote lock.

### Rule 4: Smart Workload Balancing & Evaluator Queue Algorithm
When the Chairman opens the evaluator picker, candidates are filtered by `SpecializationID`, pass the 63-year retirement age check, pass the anti-conflict filter, and are ordered by:

- **Tier 1 (Highest):** `TotalAssignedCount == 0`
- **Tier 2 (Medium):** `ActiveUnderEvaluationCount == 0`, ordered by oldest `LastAssignedDate` ascending
- **Tier 3 (Lowest):** `ActiveUnderEvaluationCount > 0`, ordered by load count ascending. UI appends a badge showing the active load count.

### Rule 5: Multi-Evaluator Scoring Formula
If multiple evaluators grade the same item:
$$\text{Final Score} = \left( \frac{\sum \text{Evaluator Scores}}{\text{Number of Evaluators}} \times 0.7 \right) + \text{Chairman Score (Max 30)}$$

Computed in application service with `DECIMAL(5,2)` precision. Stored in `ResearchPapers.FinalScore`.

### Rule 6: Immutable Minutes Freezing & System Auditing
- Once `MeetingMinutes.Status` transitions to `Minutes_Frozen`, the backend throws `FrozenMinutesException` (HTTP 423) on any PUT/POST targeting associated `ResearchPapers`, `Evaluations`, `ChairmanScores`, or `TriageMappings`.
- `FreezeEvents` table records who froze, when, and the meeting minutes ID.
- Audit trail captures `OldValueJSON` and `NewValueJSON` for every modification.

### Rule 7: Advanced Evaluator SLA Scheduling & Status Locking
- **SLA:** Default 14 days from `AssignedDate`. Warning at 10 days.
- **Breach Actions:** Re-send automated reminder email, invoke `Evaluator Substitution` routine (creates new assignment + `Substitutions` record), or enforce `EvaluatorStatus` locking: `[Active, Suspended_Permanently, Suspended_Temporarily]`.
- **Retirement Auto-Lockout:** Daily background job scans `AppUsers.BirthDate`. If `DATEDIFF(YEAR, BirthDate, GETDATE()) >= 63`, auto-set `EvaluatorStatus = Suspended_Temporarily` and emit `RetirementTriggeredEvent`.

### Rule 8: Interactive Collaboration Framework
- **SignalR Real-Time:** Live badges for state changes, meeting declarations, SLA breaches.
- **Role-Isolated Chat:** `ChatChannels` table defines `Committee_Internal` and `Evaluator_Admin_Anonymous`. Direct Evaluator-to-Researcher messaging is **hard-blocked at the hub level** (reject if `SenderRole == Evaluator && ReceiverRole == Researcher`).
- **RSVP Portal:** `MeetingRSVPs` forces `Justification` string (min 10 chars) when `Response = Decline`.

---

## 7. DOMAIN EVENTS & OUTBOX PATTERN

All state mutations and cross-cutting concerns publish domain events captured in `OutboxMessages` and dispatched by the `OutboxDispatcherJob` (Quartz, every 30 seconds).

| Event | Publisher | Subscribers |
|---|---|---|
| `ResearchSubmitted` | `ResearchPaper` aggregate | `AuditLogHandler`, `NotificationHandler`, `TriageQueueHandler` |
| `ResearchStateChanged` | `ResearchPaper` aggregate | `SignalRHub`, `AuditLogHandler`, `SLAHandler`, `NotificationHandler` |
| `EvaluatorAssigned` | `EvaluatorAssignment` | `EmailDispatcher`, `SLAHandler`, `NotificationHandler` |
| `SLABreached` | `SLABreachScannerJob` | `NotificationHandler`, `EmailDispatcher`, `EvaluatorStatusHandler` |
| `MinutesFrozen` | `MeetingMinutes` aggregate | `ResearchLockHandler`, `SignalRHub`, `AuditLogHandler` |
| `PlagiarismDetected` | `PlagiarismService` | `ResearchLockHandler`, `NotificationHandler`, `AuditLogHandler` |
| `UserVerified` | `HRVerificationQueue` | `IdentityHandler`, `NotificationHandler`, `EmailDispatcher` |
| `RetirementTriggered` | `RetirementAgeScannerJob` | `EvaluatorStatusHandler`, `NotificationHandler` |
| `VoteCast` | `Votes` | `SignalRHub`, `AuditLogHandler` |
| `BatchResponseReceived` | `MinistryBatchPollerJob` | `ResearchStateHandler`, `NotificationHandler` |
| `DelegationGranted` | `DelegatedRoles` | `AuditLogHandler`, `NotificationHandler` |
| `DelegationRevoked` | `DelegatedRoles` | `AuditLogHandler`, `NotificationHandler` |

**Outbox Guarantees:** At-least-once delivery. `RetryCount` capped at 5. Dead-letter to `EmailLogs` after exhaustion.

---

## 8. BACKGROUND ORCHESTRATION (QUARTZ.NET JOB REGISTRY)

| Job | Schedule | Responsibility |
|---|---|---|
| `SLABreachScannerJob` | Every 6 hours | Scan `EvaluatorAssignments` where `DueDate < NOW` and `SubmittedDate IS NULL`; set `IsSLABreached`; emit `SLABreached` event |
| `RetirementAgeScannerJob` | Daily at 02:00 UTC | Scan `AppUsers` where age >= 63; set `EvaluatorStatus = Suspended_Temporarily`; emit `RetirementTriggered` |
| `PlagiarismLockoutExpiryJob` | Daily at 03:00 UTC | Scan `PlagiarismLockouts` where `LockedUntil < NOW`; auto-transition research to `Fail_Rejected` |
| `OutboxDispatcherJob` | Every 30 seconds | Poll `OutboxMessages` where `ProcessedAt IS NULL`; deserialize; route to handlers; mark processed |
| `EmailDispatcherJob` | Triggered by event | Send reminder emails, 2FA OTP codes, notification digests |
| `MinistryBatchPollerJob` | Every 12 hours | Check `MinistryBatches` for response files via SFTP; parse and emit `BatchResponseReceived` |
| `DataRetentionJob` | Weekly at 04:00 UTC | Archive `AuditLogs` > 2 years to cold storage; purge `ChatMessages` > 3 years; hard-delete `Notifications` > 1 year; purge processed `OutboxMessages` > 30 days; purge rejected `HRVerificationQueue` > 90 days; purge expired `PasswordResetTokens` > 15 minutes; purge expired `FileAccessTokens` > 24 hours |
| `SessionCleanupJob` | Daily at 01:00 UTC | Revoke expired refresh tokens; clear orphaned device fingerprints |

---

## 9. TRANSACTION & CONCURRENCY STRATEGY

- **Unit of Work:** `IUnitOfWork` wraps `DbContext`. `BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`.
- **Saga Pattern:** Multi-step operations (Triage → Assign → Notify → Audit) use UoW + Outbox. The Outbox write is atomic with the business data write.
- **Optimistic Concurrency:** `RowVersion` on `ResearchPapers`, `MeetingMinutes`, `EvaluatorAssignments`, `TriageMappings`. On `DbUpdateConcurrencyException`, return HTTP 409 with the refreshed entity state.
- **PII Protection:** `NationalID` and `EmployeeID` use SQL Server **Always Encrypted** with deterministic encryption to support equality lookups.

---

## 10. CACHING STRATEGY

Use **Redis** for:
- Evaluator workload queries and tiered queue results
- Dashboard statistics and KPI snapshots
- Permission lookups and role delegation caches
- Session metadata and device fingerprints
- Notification unread counts

**Cache Policies:**
- Sliding expiration: 20 minutes for permission lookups
- Absolute expiration: 60 minutes for workload queues
- Cache invalidation on `EvaluatorAssigned`, `UserVerified`, `DelegationGranted/Revoked` events
- Distributed cache synchronization via Redis pub/sub

---

## 11. MESSAGE QUEUE & BACKGROUND PROCESSING GOVERNANCE

Implement:
- **Retry policies:** Exponential backoff (2^attempt × 30 seconds), max 5 retries
- **Dead-letter queues:** Failed Outbox messages routed to `EmailLogs` with error context
- **Idempotency:** All event handlers validate `EventId` against processed set before execution
- **Delayed jobs:** Reminder emails scheduled at 10-day SLA mark via Hangfire
- **Scheduled jobs:** All Quartz jobs registered with persistent store (SQL Server job store)
- **Job monitoring:** Hangfire Dashboard protected behind Admin-only authorization

---

## 12. FILE MANAGEMENT ARCHITECTURE

### 12.1 File Storage
- Secure FTP/SFTP backend proxy streaming
- No direct frontend exposure of internal paths
- `ResearchAttachments.FilePath` stores internal FTP path only
- `FileAccessTokens` table issues secure temporary tokens for proxy access (24-hour expiry)

### 12.2 File Security
- SHA-256 hashing for integrity verification
- MIME validation against whitelist
- Extension validation: `.pdf`, `.doc`, `.docx`, `.xlsx`, `.png`, `.jpg`
- Antivirus scanning (ClamAV daemon) before persistence
- File versioning: `VersionNumber` and `IsLatestVersion` on `ResearchAttachments`
- File size limit: 50MB per upload
- Secure temporary access tokens for download proxy

---

## 13. REPORTING & ANALYTICS

### 13.1 Reporting Engine
- **QuestPDF** for immutable PDF generation (Meeting Minutes, Research Certificates)
- **FastReport** for operational report templates
- **ClosedXML** for Excel exports (SLA grids, Evaluator portfolios)

### 13.2 Analytics Dashboard (Screen 33)
Provide high-level charts for the Director General:
- Research throughput metrics (submitted / approved / rejected per quarter)
- SLA statistics (average evaluation time, breach rate, overdue percentage)
- Evaluator performance (average scores, on-time rate, workload distribution)
- Committee efficiency (meetings held, average time to decision)
- Historical trends (3-year rolling comparison)
- Department-level research distribution

### 13.3 Charting
- **ApexCharts** for interactive dashboards
- **Chart.js** for lightweight embedded widgets

---

## 14. LOCALIZATION & GLOBALIZATION

### 14.1 Languages
- **Arabic (RTL)** — primary operational language
- **English (LTR)** — secondary administrative language

### 14.2 Localization Stack
- `ngx-translate` with lazy-loaded resource JSON files
- Angular i18n support for date/number/currency formatting
- Arabic normalization for search (remove tashkeel, normalize alef variants)
- Unicode-safe full-text search
- Hijri date display alongside Gregorian

### 14.3 Typography
- **Tajawal** for Arabic headings and UI labels
- **Segoe UI** for English text and data grids

---

## 15. LOGGING & EXCEPTION HANDLING

### 15.1 Logging
- **Serilog** with structured JSON output
- **Correlation IDs** propagated across all layers (Angular → API → Jobs → SQL)
- **Request tracing** with OpenTelemetry spans
- Log enrichment: `{TraceId}`, `{SpanId}`, `{UserId}`, `{EmployeeID}`, `{Role}`

### 15.2 Exception Handling
- **Global exception middleware** catching all unhandled exceptions
- **RFC 7807 ProblemDetails** for standardized API error responses
- **Validation exception mapping** via FluentValidation to ProblemDetails
- **Security exception handling** for auth failures, rate limit breaches, frozen minutes violations
- **Concurrency exception handling** returning HTTP 409 with current entity snapshot

---

## 16. HEALTH MONITORING

### 16.1 Health Checks
Expose endpoint `/health` returning:
- SQL Server connectivity
- Redis connectivity
- FTP/SFTP connectivity
- SMTP/Email service connectivity
- SignalR hub connectivity
- Quartz job store connectivity

### 16.2 Monitoring Stack
- **Prometheus** metrics collection
- **Grafana** dashboards for infrastructure and business metrics
- **Application Insights** for distributed tracing and dependency mapping

---

## 17. DATA RETENTION & ARCHIVAL

| Data | Retention | Action |
|---|---|---|
| `AuditLogs` | 7 years | Partitioned by year; archive to cold storage after 2 years |
| `ChatMessages` | 3 years | Soft delete then archive |
| `Notifications` | 1 year | Hard delete |
| `ResearchPapers` (Archived) | 10 years | Then move to read-only archive database |
| `ResearchAttachments` | 10 years post-archival | Then cold storage |
| `HRVerificationQueue` (Rejected) | 90 days | Hard delete |
| `PasswordResetTokens` | 15 minutes | Auto-purge |
| `FileAccessTokens` | 24 hours | Auto-purge |
| `OutboxMessages` (Processed) | 30 days | Hard delete |
| `SystemConfigurations` | Indefinite | History tracked in `SystemConfigurationHistory` |
| `EmailLogs` | 2 years | Archive to cold storage after 1 year |

**Archival Strategy:**
- Archived data moved to cold storage (Azure Blob Archive / S3 Glacier)
- Partitioned by year for `AuditLogs` and `ResearchPapers`
- Excluded from active dashboards and search indexes after archival
- Point-in-time recovery supported for 30 days via SQL Server backups

---

## 18. THE COMPLETE MASTER UI/UX SCREEN MATRIX (33 SCREENS)

Implement using the **Muted Industrial Style Guide**: Oil Blue `#0F2A38` and `#163E54`, Slate Gray `#4A607A`, Off-White `#F4F6F9`, Tajawal / Segoe UI, `border-radius: 4px-6px`, `0.2s ease-in-out` transitions. Full Arabic RTL support. Hijri date display. WCAG 2.1 AA accessibility.

#### Portfolio 1: Identity & Security UX
1. **Secure Login Screen** — JWT handler, 2FA conditional redirect, device fingerprint capture
2. **Employee Sign-Up Screen** — HR metadata capture, validation, department/directorate selection
3. **HR Verification Approval Dashboard** — Admin/Secretary approve/reject queue with rejection reason
4. **Password Reset Portal** — 15-minute tokenized link via `PasswordResetTokens`
5. **Two-Factor Authentication (2FA) Verification Screen** — TOTP input, QR enrollment, backup codes

#### Portfolio 2: Profile & Personalization
6. **User Profile Page** — HR metadata, historic research tree, delegated roles view
7. **Onboarding Tour Guides** — First-login interactive manual per role (Researcher, Evaluator, Chairman)
8. **System Preferences Panel** — AR/EN toggle, email options, Light/Dark mode, notification preferences

#### Portfolio 3: Researcher & Submission
9. **Research Submission Wizard** — Stepper form with `ReplacedResearchID` version linkage, category/specialization selection, FTP upload
10. **Interactive Research Timeline Viewer** — Horizontal flowchart of current active state with Arabic labels
11. **Correction Submission Interface** — Document correction for `Non_Compliant_Returned` with secretary notes view
12. **Researcher Personal History View** — All past papers, substitutions, corrections, and final scores

#### Portfolio 4: Incoming Triage & Committee Management
13. **Incoming Research Triage Dashboard** — Chairman/Supervisor notification hub with SignalR live badges
14. **Triage Action Dynamic Panel** — Inline mapping for members + workload-balanced evaluators with swap/remove controls
15. **Evaluator Workload Optimization Dropdown** — Tiered list (Tier 1/2/3) with active load badges and specialization filters
16. **Committee Member Workspace** — Assigned papers grid for formatting, technical report reviews, and score entry
17. **Member/Supervisor Personal History View** — Sessions attended, reports graded, minutes supervised, delegation history

#### Portfolio 5: Governance & SLA Tracking
18. **SLA 10-Days Violation Grid** — Live tracking with reminder/substitution/suspension triggers
19. **Evaluator Roster & Status Manager** — Toggle Active/Suspended/Temporarily Absent with retirement flags
20. **Plagiarism Lockout Override Console** — Chairman panel to lift 3-month bans with justification document upload
21. **System Master Audit Log Viewer** — JSON differential viewer with OldValue/NewValue comparison for Admins
22. **HR Metadata Dictionary Manager** — Lookup tables for specializations, directorates, departments, title mappings
23. **Evaluator Historic Portfolio View** — All historic papers evaluated, grading records, SLA performance trends

#### Portfolio 6: Sessions, Collaboration & External Gateways
24. **Meeting Scheduler & Agenda Builder** — Secretary form, FTP agenda upload, attendee multi-select, date picker
25. **Meeting Portal RSVP Screen** — Accept/Decline with forced justification validator (min 10 chars if Decline)
26. **RSVP Real-Time Monitor** — Graphical attendee tracking chart for Chairman (accepted/declined/pending counts)
27. **Meeting Minutes Studio & Live Editor** — Rich editor compiling the **5 BOC Sections** into an immutable PDF
28. **Committee Internal Chat Box** — SignalR real-time messaging workspace for committee members
29. **Evaluator Anonymous Helpdesk Chat** — Secure isolated routing to Admin roles, hard-blocked from researchers
30. **In-App Notification Sidebar Center** — Sliding drawer caching and routing live deep-linked alerts
31. **System Notifications Audit Log** — Infrastructure log verifying email deliveries and hub broadcast receipts
32. **Future-Proof Ministry Gateway** — Secure portal for individual file processing and batch response ingestion
33. **Executive Analytics Dashboard** — High-level charts for the Director General (throughput, SLA, evaluator performance, committee efficiency)

**The 5 BOC Minutes Sections:**
1. Session Opening & Attendance Declaration
2. Research Review & Evaluator Reports Summary
3. Committee Discussion & Deliberation Record
4. Voting Results & Chairman Grading
5. Final Decision & Signatures

---

## 19. TESTING ARCHITECTURE

| Layer | Framework | Scope |
|---|---|---|
| **Unit Tests** | xUnit + Moq + FluentAssertions | Domain logic, state machine, scoring formula, permission checks, FSM transitions, value objects |
| **Integration Tests** | xUnit + WebApplicationFactory + Testcontainers (SQL Server) | API endpoints, repository queries, SignalR hubs, Quartz jobs, Outbox dispatch, FluentFTP proxy |
| **E2E Tests** | Playwright | Critical journeys: submit → triage → evaluate → freeze → archive → ministry gateway |
| **Load Tests** | k6 | Triage dashboard under 50 concurrent Chairman users; SignalR chat under 200 concurrent connections |

**Critical Test Cases:**
- Illegal FSM transitions throw `InvalidStateTransitionException` with correct HTTP 409
- Anti-conflict filter excludes same-department AND same-directorate evaluators
- 63-year retirement scanner flags exactly on birthdate anniversary
- Minutes freeze blocks all mutating HTTP (PUT/POST) on associated entities with HTTP 423
- Score formula precision to 2 decimal places: `(mean * 0.7) + chairmanScore`
- FTP proxy never leaks internal path in response headers, JSON, or error messages
- Outbox guarantees at-least-once delivery under simulated SQL restart
- RowVersion concurrency returns HTTP 409 with current entity state snapshot
- Delegated role permissions expire correctly after `ValidUntil`
- File access tokens expire after 24 hours and reject subsequent access

---

## 20. STEP-BY-STEP IMPLEMENTATION PIPELINE (PHASED EXECUTION)

Execute chronologically. Do not mix components or output truncated blocks.

#### PHASE A: DATABASE LAYER (Distributed Architecture & EF Core)
Generate full, optimized SQL DDL scripts, Entity Configurations via Fluent API, and DbContext templates for **all 35+ tables** including Identity (with Supervisor flags, 2FA, device tracking, delegation), HR validation queue, Multi-Evaluator mapping, Specialization lookups, Audit Logging, Tri-Centric History tables, OutboxMessages, TriageMappings, Votes, MinistryBatches, SystemConfigurationHistory, FileAccessTokens, DelegatedRoles, and PermissionScopes. Ensure `Guid` primary keys, `RowVersion` concurrency, `DeleteBehavior.Restrict`, computed columns, full-text indexes, filtered indexes, covering indexes, and Always Encrypted on PII.

#### PHASE B: BACKEND LAYER (ASP.NET Core Web API Core Engine)
Provide production-ready C# code components (Controllers, Application Services, Repositories, CQRS Handlers, DTOs, Validators, SignalR Hubs, Quartz Jobs, Hangfire Tasks, Domain Event Handlers) implementing:
- Admin omnipotence overrides
- Self-Assignment evaluation logic with specialization alignment
- Native FluentFTP streaming proxy with SHA-256 integrity and `FileAccessTokens`
- Background tasks: SLA breach scanner, retirement age scanner, plagiarism expiry, outbox dispatcher, ministry batch poller, data retention, session cleanup
- Tri-Centric History log aggregators (Researcher, Member, Evaluator views)
- Immutable frozen minutes validation handler
- Outbox pattern with domain event routing and idempotency
- Unit of Work with transaction boundaries and saga orchestration
- Global exception middleware returning RFC 7807 ProblemDetails
- Delegated role permission evaluation
- Device fingerprint validation on token refresh

#### PHASE C: FRONTEND LAYER (Angular SPA Components & Style System)
Provide complete TypeScript modules, standalone component classes, layout services, and SCSS styling implementing the **Muted Industrial Style Guide** for:
- Incoming Triage Dashboard with SignalR live badges
- Supervisor Operations Portal with delegation controls
- HR Verification Approval Dashboard
- Tri-Centric History Data Grids
- Workload-prioritized Evaluator Dropdown Picker with active load badges
- Meeting Minutes Studio with the 5 BOC Sections
- Ministry Gateway interface
- Executive Analytics Dashboard with ApexCharts
- Full Arabic RTL, Hijri dates, Tajawal font, WCAG 2.1 AA

#### PHASE D: DEVOPS & DEPLOYMENT LAYER
Provide production-ready infrastructure artifacts:
- **Dockerfile** (multi-stage build: SDK → runtime)
- **docker-compose.yml** (API + SQL Server + Redis + NGINX + Hangfire Dashboard)
- **Kubernetes manifests** (Deployment, Service, Ingress, ConfigMap, Secret, HPA)
- **CI/CD Pipelines** (Azure DevOps YAML or GitHub Actions): build → test (xUnit + Playwright) → containerize → deploy to staging → approval gate → deploy to production
- **Environment configs** (appsettings.Development.json, appsettings.Staging.json, appsettings.Production.json)
- **NGINX reverse proxy config** (SSL termination, rate limiting headers, static file caching)
- **Health check probes** (liveness, readiness, startup)
- **Prometheus/Grafana configs** (service monitor, dashboard JSON)

---

## 21. INTERACTION MANDATE

You must:
- Act as a Lead Enterprise Architect
- Preserve architectural consistency across all phases
- Avoid abbreviated implementations
- Generate production-grade, boilerplate-inclusive code only
- Respect all enterprise constraints (security, auditability, transactional integrity, distributed safety)
- Maintain the 14-state FSM governance strictly
- Preserve the permission matrix enforcement
- Honor the Outbox pattern guarantees
- Implement the 33-screen UI matrix completely

Acknowledge your functional role as Lead Architect. Confirm your absolute understanding of this unified master enterprise blueprint, including:
- Admin omnipotence and Supervisor elevation
- Executive Self-Assignment with specialization alignment
- The 14 operational domain states and their legal transition matrix
- The Outbox pattern, domain event catalog, and idempotency guarantees
- The FTP streaming proxy with `FileAccessTokens` and file versioning
- The 63-year age constraint and auto-lockout
- The 5 BOC Minutes Sections
- The 33 comprehensive user interface screens
- The full permission matrix, delegated roles, and security hardening mandate
- The background job registry, retention policies, and archival strategy
- The DevOps pipeline (Docker, K8s, CI/CD, monitoring)

**Await my first command specifying the target layer and step to execute** (e.g., "Begin Phase A: Database Layer Configuration"). Provide clean, complete, boilerplate-inclusive code blocks without abbreviations.
