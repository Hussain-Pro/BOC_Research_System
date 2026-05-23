# BOC Research Evaluation System — v11 Project Status & Gap Analysis
**Generated:** 2026-05-23 | **Blueprint Reference:** `BOC_Research_System_v11_Unified_Master_Blueprint.md`

---

## Executive Summary

| Category | Status |
|---|---|
| **Phase A – Database DDL** | ✅ Complete (54 KB SQL script) |
| **Phase B – Backend (Domain)** | 🟡 Partial — Core done, several services & jobs missing |
| **Phase B – Backend (Application/CQRS)** | 🟡 Partial — 4 feature domains, limited handlers |
| **Phase B – Backend (Infrastructure)** | 🟡 Partial — Core wiring done, 5 missing jobs |
| **Phase B – Backend (WebAPI)** | 🟡 Partial — 4 controllers, no health/reporting endpoints |
| **Phase C – Frontend (Angular)** | 🔴 Early — 4 of 33 screens scaffolded |
| **Phase D – DevOps** | 🔴 Not started |
| **Testing** | 🟡 Unit tests only (11 cases) |

---

## PHASE A: DATABASE LAYER ✅

### Status: COMPLETE

**File:** `database/BOC_PhaseA_SQL_DDL_v11_FIXED.sql` (54,540 bytes)

All 35+ tables generated including:
- Identity & Directory tables (`AppUsers`, `AppRoles`, `Directorates`, `Departments`, `Specializations`)
- Research Lifecycle tables
- Meeting & Governance tables
- Triage & Evaluation tables
- Collaboration & Notifications tables
- Ministry & Batch Processing tables
- Audit, System & HR tables
- `DelegatedRoles`, `PermissionScopes`, `FileAccessTokens` (NEW entities)
- Full-text indexes on `ResearchPapers.Title` and `Abstract`
- `NEWSEQUENTIALID()` Guid primary keys
- `RowVersion` optimistic concurrency
- Computed/persisted columns (`IsArchived`, `IsFrozen`, `IsSLABreached`)
- Always Encrypted columns on `NationalID` and `EmployeeID`

**Remaining Concern:** `appsettings.json` does not contain the full connection string with `Column Encryption Setting=enabled`. This is currently hard-coded only in `DependencyInjection.cs` as a fallback.

---

## PHASE B: BACKEND LAYER

### B1 — Domain Layer (`BOC.Domain`) 🟡

#### ✅ Implemented
| Component | Files |
|---|---|
| **All 40 Entity Classes** | `Entities/` — All 40 entity files present |
| **All 12 Enum Definitions** | `Enums/` — `ResearchState`, `AccountStatus`, `EvaluatorStatus`, `MeetingStatus`, `MeetingMinutesStatus`, `AssignmentStatus`, `VoteValue`, `ChannelType`, `ActionType`, `HRVerificationStatus`, `MinistryBatchStatus`, `EmailDeliveryStatus` |
| **5 Domain Events** | `Events/` — `ResearchSubmittedEvent`, `EvaluatorAssignedEvent`, `SLABreachedEvent`, `MinutesFrozenEvent`, `RetirementTriggeredEvent` |
| **2 Value Objects** | `ValueObjects/` — `EmployeeIdentity`, `FileHash` |
| **ResearchStateMachine (FSM)** | `Fsm/ResearchStateMachine.cs` — Full 14-state FSM |
| **ResearchScoringService** | `Services/ResearchScoringService.cs` — 70/30 formula |
| **2 Domain Exceptions** | `Exceptions/` — `InvalidStateTransitionException`, `FrozenMinutesException` |
| **Entity Base Class** | `Common/Entity.cs` |

#### ❌ Missing from Blueprint
| Missing Component | Blueprint Reference |
|---|---|
| `EvaluatorWorkloadService` | Section 3.3 Domain Services |
| `SLAEscalationService` | Section 3.3 Domain Services |
| `AssignmentConflictService` | Section 3.3 Domain Services |
| `MeetingFreezeService` | Section 3.3 Domain Services |
| 4 more Value Objects | `ResearchScore`, `MeetingStatus` (as VO), `EvaluationResult`, `ResearchState` (as VO) |
| 7 more Domain Events | `ResearchStateChangedEvent`, `PlagiarismDetectedEvent`, `UserVerifiedEvent`, `VoteCastEvent`, `BatchResponseReceivedEvent`, `DelegationGrantedEvent`, `DelegationRevokedEvent` |

---

### B2 — Application Layer (`BOC.Application`) 🟡

#### ✅ Implemented
| Feature Domain | Commands | Queries |
|---|---|---|
| **Auth** | `LoginUserCommand`, `RegisterUserCommand`, `VerifyTwoFactorCommand` | — |
| **Research** | `SubmitResearchCommand`, `TriageAssignCommand` | `GetResearchDocumentQuery` |
| **Meetings** | `CastVoteCommand`, `FreezeMinutesCommand`, `SubmitChairmanScoreCommand` | `GetMeetingDetailsQuery` |
| **Triage** | — | `GetEligibleEvaluatorsQuery`, `GetTriagePapersQuery` |

Pipeline Behaviors: `ValidationBehavior`, `TransactionBehavior`

#### ❌ Missing CQRS Handlers (High Priority)
| Missing Handler | Needed For |
|---|---|
| `SubmitEvaluationCommand` | Evaluator score submission |
| `RefreshTokenCommand` | Token refresh flow |
| `LogoutCommand` | Session revocation |
| `ResetPasswordCommand` + `RequestPasswordResetCommand` | Password reset portal |
| `CreateMeetingCommand` | Meeting scheduler (Screen 24) |
| `UpdateTriageMappingCommand` | Dynamic swap/remove from triage |
| `FinalizeTriageMappingCommand` | Lock triage before vote |
| `SubmitRsvpCommand` | Meeting RSVP (Screen 25) |
| `UploadMeetingAgendaCommand` | FTP agenda upload |
| `SubmitCorrectionCommand` | Screen 11 — correction resubmit |
| `GetResearchHistoryQuery` | Screen 12 — researcher history |
| `GetSLADashboardQuery` | Screen 18 — SLA grid |
| `GetEvaluatorRosterQuery` | Screen 19 |
| `GetAuditLogsQuery` | Screen 21 |
| `GetHRVerificationQueueQuery` | Screen 3 |
| `ApproveHRVerificationCommand` | Screen 3 |
| `GetMeetingListQuery` | Meeting list/scheduler |
| `GetNotificationsQuery` | Notification sidebar |
| `MarkNotificationReadCommand` | Notification management |
| `SendChatMessageCommand` | Chat feature |
| `GetMinistryBatchesQuery` | Screen 32 |
| `CreateMinistryBatchCommand` | Ministry gateway |
| `LiftPlagiarismLockoutCommand` | Screen 20 |
| `GetExecutiveAnalyticsQuery` | Screen 33 — director dashboard |
| `GetEvaluatorWorkloadQuery` | Tier 1/2/3 workload picker |

#### ❌ Missing Application Infrastructure
| Missing | Blueprint Reference |
|---|---|
| AutoMapper Profiles | Section 1.1 — AutoMapper for DTO projection |
| `IResearchRepository` interface | Needed by handlers |
| `IMeetingRepository` interface | Needed by handlers |
| Hangfire task registrations | Section 11 — delayed emails |
| Outbox event routing & idempotency checks | Section 7 |

---

### B3 — Infrastructure Layer (`BOC.Infrastructure`) 🟡

#### ✅ Implemented
| Component | File |
|---|---|
| `BOCDbContext` | `Persistence/BOCDbContext.cs` |
| Unit of Work | `Persistence/UnitOfWork.cs` |
| `AuditLogInterceptor` | `Persistence/Interceptors/AuditLogInterceptor.cs` |
| `OutboxInterceptor` | `Persistence/Interceptors/OutboxInterceptor.cs` |
| 10 EF Core Configurations | `Persistence/Configurations/` |
| `RedisCacheService` | `Caching/RedisCacheService.cs` |
| `SmtpEmailSender` | `Emails/SmtpEmailSender.cs` |
| `FtpFileStorageService` | `Storage/FtpFileStorageService.cs` |
| `JwtTokenGenerator` | `Security/JwtTokenGenerator.cs` |
| `PasswordHasher` | `Security/PasswordHasher.cs` |
| `JwtTokenGenerator` | `Security/JwtTokenGenerator.cs` |
| `PasswordHasher` | `Security/PasswordHasher.cs` |
| `TotpService` | `Security/TotpService.cs` |
| Quartz Jobs (8/8) | `OutboxDispatcherJob`, `SLABreachScannerJob`, `RetirementAgeScannerJob`, `PlagiarismLockoutExpiryJob`, `EmailDispatcherJob`, `MinistryBatchPollerJob`, `DataRetentionJob`, `SessionCleanupJob` |

#### ✅ Completed Quartz Jobs (8/8)
| Job | Schedule | Blueprint Ref |
|---|---|---|
| `PlagiarismLockoutExpiryJob` | Daily 03:00 UTC | Section 8 |
| `EmailDispatcherJob` | Event-triggered | Section 8 |
| `MinistryBatchPollerJob` | Every 12 hours | Section 8 |
| `DataRetentionJob` | Weekly 04:00 UTC | Section 8 |
| `SessionCleanupJob` | Daily 01:00 UTC | Section 8 |

#### ❌ Missing Infrastructure Services
| Missing | Blueprint Reference |
|---|---|
| `FileProxyController` / `FileAccessTokenService` | Section 12 — secure proxy with `FileAccessTokens` |
| `ClamAV` antivirus integration | Section 12.2 |
| `QuestPDF` report generator | Section 13.1 |
| `ClosedXML` Excel export | Section 13.1 |
| Health check implementations (SQL/FTP/Redis/SMTP) | Section 16.1 |
| Rate limiting middleware (`AspNetCoreRateLimit`) | Section 2.1 |
| Hangfire setup & dashboard | Section 11 |
| `DelegatedRoleEvaluator` (runtime delegation) | Section 2.4 |
| Permission policy handlers | Section 2.2 |
| OpenTelemetry/Jaeger exporter | Section 15.1 |
| Prometheus metrics integration | Section 16.2 |

---

### B4 — WebAPI Layer (`BOC.WebAPI`) 🟡

#### ✅ Implemented
| Component | File |
|---|---|
| `AuthController` | Login, Register, 2FA Verify, RefreshToken, ChangePassword, ForgotPassword, ResetPassword |
| `ResearchController` | Submit, TriageAssign, GetDocument |
| `MeetingsController` | CastVote, FreezeMinutes, SubmitChairmanScore, GetDetails |
| `TriageController` | GetTriagePapers, GetEligibleEvaluators |
| `HRVerificationController` | Queue and approval endpoints |
| `ProfileController` | User profile settings and theme |
| `NotificationsController` | Get notifications and mark as read |
| `FileProxyController` | Secure FTP streaming with tokens |
| `GlobalExceptionMiddleware` | RFC 7807 ProblemDetails |
| `TwoFactorEnforcementMiddleware` | 2FA gate |
| `ChatHub` (SignalR) | Role-isolated chat with anti-researcher block |
| `NotificationHub` (SignalR) | Push notifications |

#### ❌ Missing Controllers & Endpoints
| Missing Controller | Screens / Features |
|---|---|
| `ChatController` | Screen 28, 29 |
| `SLAController` | Screen 18 |
| `EvaluatorRosterController` | Screen 19, 20, 23 |
| `AuditLogController` | Screen 21 |
| `SystemConfigController` | Screen 22 |
| `MinistryGatewayController` | Screen 32 |
| `AnalyticsController` | Screen 33 |
| `ReportController` | Section 13 — PDF/Excel generation |

---

## PHASE C: FRONTEND LAYER (`BOC.Frontend` Angular SPA) 🔴

### Implemented Screens: 4 of 33

| # | Screen | Status | Component Path |
|---|---|---|---|
| 1 | Secure Login Screen | ✅ | `pages/login/` |
| 9 | Research Submission Wizard | ✅ | `pages/submit-research/` |
| 13 | Incoming Research Triage Dashboard | ✅ | `pages/triage-dashboard/` |
| 27 | Meeting Minutes Studio & Live Editor | ✅ | `pages/meeting-studio/` |

### Angular Services Implemented
| Service | File |
|---|---|
| `AuthService` | `services/auth.service.ts` |
| `ResearchService` | `services/research.service.ts` |
| `TriageService` | `services/triage.service.ts` |
| `MeetingService` | `services/meeting.service.ts` |
| `SignalRService` | `services/signalr.service.ts` |

### ❌ Missing Screens (29 of 33)
| # | Screen Name |
|---|---|
| 2 | Employee Sign-Up Screen |
| 3 | HR Verification Approval Dashboard |
| 4 | Password Reset Portal |
| 5 | Two-Factor Authentication (2FA) Verification Screen |
| 6 | User Profile Page |
| 7 | Onboarding Tour Guides |
| 8 | System Preferences Panel |
| 10 | Interactive Research Timeline Viewer |
| 11 | Correction Submission Interface |
| 12 | Researcher Personal History View |
| 14 | Triage Action Dynamic Panel |
| 15 | Evaluator Workload Optimization Dropdown |
| 16 | Committee Member Workspace |
| 17 | Member/Supervisor Personal History View |
| 18 | SLA 10-Days Violation Grid |
| 19 | Evaluator Roster & Status Manager |
| 20 | Plagiarism Lockout Override Console |
| 21 | System Master Audit Log Viewer |
| 22 | HR Metadata Dictionary Manager |
| 23 | Evaluator Historic Portfolio View |
| 24 | Meeting Scheduler & Agenda Builder |
| 25 | Meeting Portal RSVP Screen |
| 26 | RSVP Real-Time Monitor |
| 28 | Committee Internal Chat Box |
| 29 | Evaluator Anonymous Helpdesk Chat |
| 30 | In-App Notification Sidebar Center |
| 31 | System Notifications Audit Log |
| 32 | Future-Proof Ministry Gateway |
| 33 | Executive Analytics Dashboard |

### ❌ Missing Angular Infrastructure
| Missing | Blueprint Reference |
|---|---|
| `ngx-translate` i18n setup (AR/EN JSON files) | Section 14 |
| Arabic RTL layout module | Section 14 |
| `ngx-hijri-date` integration | Section 14 |
| Tajawal font + SCSS theme system | Section 18 |
| Angular route guards (AuthGuard, RoleGuard, 2FAGuard) | Section 2 |
| `ApexCharts` module integration | Section 13.3 |
| `Chart.js` integration | Section 13.3 |
| HTTP Interceptors (JWT, Correlation ID, Error handling) | Section 15 |
| Standalone component routing (lazy loading) | Section 1.2 |
| WCAG 2.1 AA accessibility compliance | Section 18 |

---

## PHASE D: DEVOPS & DEPLOYMENT 🔴

### Status: NOT STARTED

| Artifact | Status |
|---|---|
| `Dockerfile` (multi-stage) | ❌ Missing |
| `docker-compose.yml` (API + SQL + Redis + NGINX) | ❌ Missing |
| Kubernetes Deployment manifests | ❌ Missing |
| Kubernetes Service/Ingress/ConfigMap/Secret | ❌ Missing |
| GitHub Actions CI/CD pipeline | ❌ Missing |
| `appsettings.Staging.json` | ❌ Missing |
| `appsettings.Production.json` | ❌ Missing |
| NGINX reverse proxy config | ❌ Missing |
| Prometheus scrape config | ❌ Missing |
| Grafana dashboard JSON | ❌ Missing |
| Health check K8s probes | ❌ Missing |

---

## TESTING COVERAGE

| Layer | Framework | Status |
|---|---|---|
| Unit Tests | xUnit | 🟡 11 test cases — FSM + Scoring |
| Integration Tests | xUnit + Testcontainers | ❌ Not started |
| E2E Tests | Playwright | ❌ Not started |
| Load Tests | k6 | ❌ Not started |

---

## INFRASTRUCTURE CONFIGURATION REFERENCE

### SQL Server
| Setting | Value |
|---|---|
| **Host** | `127.0.0.1` |
| **Port** | `1433` |
| **Database** | `BOC_Research_Evaluation` |
| **User** | `sa` |
| **Password** | `12345` |
| **Trust Cert** | `true` |
| **Always Encrypted** | `Column Encryption Setting=enabled` |
| **Config Key** | `ConnectionStrings:DefaultConnection` |

### SMTP (Email)
| Setting | Value |
|---|---|
| **Host** | `mail.boc.oil.gov.iq` |
| **Port** | `465` |
| **Protocol** | SSL/TLS (`SslOnConnect`) |
| **From Email** | `hr-planing@boc.oil.gov.iq` |
| **Display Name** | `BOC HR Research Planning` |
| **Config Keys** | `Smtp:Host`, `Smtp:Port`, `Smtp:FromEmail`, `Smtp:Password` |

### FTP (File Storage)
| Setting | Value |
|---|---|
| **Config Keys** | `Ftp:Host`, `Ftp:Port`, `Ftp:Username`, `Ftp:Password`, `Ftp:RootDirectory` |
| **Default Port** | `21` |
| **Default Root** | `/uploads` |
| **Library** | `FluentFTP` (synchronous `FtpClient` wrapped in `Task.Run`) |
| **Fallback** | Local filesystem (`temp_ftp_uploads/`) when `Ftp:Host` is null |

### Redis Cache
| Setting | Value |
|---|---|
| **Config Key** | `Redis:ConnectionString` |
| **Default** | `localhost:6379,abortConnect=false` |
| **Fallback** | In-memory cache when Redis unavailable |
| **Backplane** | Used for SignalR multi-instance support |

### JWT Authentication
| Setting | Value |
|---|---|
| **Algorithm** | RS256 |
| **Access Token Expiry** | 15 minutes |
| **Refresh Token Expiry** | 7 days |
| **Config Keys** | `Jwt:Issuer`, `Jwt:Audience`, `Jwt:PrivateKey`, `Jwt:PublicKey` |
| **Storage** | HttpOnly Secure Cookie for refresh token |

### Quartz.NET Background Jobs
| Job | Schedule | Status |
|---|---|---|
| `OutboxDispatcherJob` | Every 30 seconds | ✅ Registered |
| `SLABreachScannerJob` | Every 6 hours | ✅ Registered |
| `RetirementAgeScannerJob` | Daily 02:00 UTC | ✅ Registered |
| `PlagiarismLockoutExpiryJob` | Daily 03:00 UTC | ✅ Registered |
| `EmailDispatcherJob` | Event-triggered | ✅ Registered |
| `MinistryBatchPollerJob` | Every 12 hours | ✅ Registered |
| `DataRetentionJob` | Weekly 04:00 UTC | ✅ Registered |
| `SessionCleanupJob` | Daily 01:00 UTC | ✅ Registered |

---

## PRIORITY IMPLEMENTATION ROADMAP

### Immediate Next Steps (Recommended Order)

**Step 1 — Complete appsettings.json** *(~1 hour)*
Populate full configuration keys for SMTP, FTP, Redis, JWT, and connection string.

**Step 2 — Missing EF Core Configurations** *(~4 hours)*
25 of 35+ entity tables still lack Fluent API configuration files.

**Step 3 — Missing Application CQRS Handlers** *(~8 hours)*
Prioritize: `SubmitEvaluationCommand`, `CreateMeetingCommand`, `SubmitRsvpCommand`, `SubmitCorrectionCommand`, `GetResearchHistoryQuery`.

**Step 4 — Missing Quartz Jobs** *(~4 hours)*
`PlagiarismLockoutExpiryJob`, `SessionCleanupJob`, `DataRetentionJob`, `MinistryBatchPollerJob`.

**Step 5 — Missing Backend Controllers** *(~2 hours)*
`SLAController`, `EvaluatorRosterController`, `AuditLogController`, `SystemConfigController`, `MinistryGatewayController`, `AnalyticsController`, `ReportController`.

**Step 6 — Angular Frontend (Remaining 29 screens)** *(~30+ hours)*
Prioritize: Sign-Up (2), 2FA Screen (5), Research Timeline (10), RSVP (25), Chat (28/29), Notifications (30).

**Step 7 — DevOps Artifacts** *(~4 hours)*
`Dockerfile`, `docker-compose.yml`, `appsettings.Production.json`.

**Step 8 — Integration & E2E Tests** *(~8 hours)*
Testcontainers SQL integration tests. Playwright E2E for critical submit→triage→evaluate→freeze journey.
