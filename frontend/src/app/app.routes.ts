import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    children: [
      { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
      { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent) },
      { path: '2fa', loadComponent: () => import('./pages/two-factor/two-factor.component').then(m => m.TwoFactorComponent) },
      { path: 'forgot-password', loadComponent: () => import('./pages/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) },
      { path: 'reset-password', loadComponent: () => import('./pages/reset-password/reset-password.component').then(m => m.ResetPasswordComponent) }
    ]
  },
  {
    path: 'profile',
    loadComponent: () => import('./pages/user-profile/user-profile.component').then(m => m.UserProfileComponent)
  },
  {
    path: 'home',
    loadComponent: () => import('./pages/home-dashboard/home-dashboard.component').then(m => m.HomeDashboardComponent)
  },
  {
    path: 'research',
    children: [
      { path: 'timeline/:id', loadComponent: () => import('./pages/research-timeline/research-timeline.component').then(m => m.ResearchTimelineComponent) },
      { path: 'submit', loadComponent: () => import('./pages/submit-research/submit-research.component').then(m => m.SubmitResearchComponent) },
      { path: 'corrections/:id', loadComponent: () => import('./pages/research-corrections/research-corrections.component').then(m => m.ResearchCorrectionsComponent) },
      { path: 'history', loadComponent: () => import('./pages/researcher-history/researcher-history.component').then(m => m.ResearcherHistoryComponent) }
    ]
  },
  {
    path: 'committee',
    children: [
      { path: 'workspace', loadComponent: () => import('./pages/committee-workspace/committee-workspace.component').then(m => m.CommitteeWorkspaceComponent) },
      { path: 'scheduler', loadComponent: () => import('./pages/meeting-scheduler/meeting-scheduler.component').then(m => m.MeetingSchedulerComponent) },
      { path: 'studio/:id', loadComponent: () => import('./pages/meeting-studio/meeting-studio.component').then(m => m.MeetingStudioComponent) },
      { path: 'rsvp', loadComponent: () => import('./pages/rsvp/rsvp.component').then(m => m.RsvpComponent) }
    ]
  },
  {
    path: 'evaluator',
    children: [
      { path: 'portfolio', loadComponent: () => import('./pages/evaluator-portfolio/evaluator-portfolio.component').then(m => m.EvaluatorPortfolioComponent) }
    ]
  },
  {
    path: 'admin',
    children: [
      { path: 'notifications', loadComponent: () => import('./pages/notifications/notifications.component').then(m => m.NotificationsComponent) },
      { path: 'chat', loadComponent: () => import('./pages/chat/chat.component').then(m => m.ChatComponent) },
      { path: 'analytics', loadComponent: () => import('./pages/admin/analytics-dashboard/analytics-dashboard.component').then(m => m.AnalyticsDashboardComponent) },
      { path: 'roster', loadComponent: () => import('./pages/admin/evaluator-roster/evaluator-roster.component').then(m => m.EvaluatorRosterComponent) },
      { path: 'ministry', loadComponent: () => import('./pages/admin/ministry-gateway/ministry-gateway.component').then(m => m.MinistryGatewayComponent) },
      { path: 'audit', loadComponent: () => import('./pages/admin/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent) },
      { path: 'config', loadComponent: () => import('./pages/admin/system-config/system-config.component').then(m => m.SystemConfigComponent) },
      { path: 'violations', loadComponent: () => import('./pages/admin/sla-violations/sla-violations.component').then(m => m.SlaViolationsComponent) },
      { path: 'plagiarism', loadComponent: () => import('./pages/admin/plagiarism-override/plagiarism-override.component').then(m => m.PlagiarismOverrideComponent) },
      { path: 'search', loadComponent: () => import('./pages/admin/global-search/global-search.component').then(m => m.GlobalSearchComponent) },
      { path: 'export', loadComponent: () => import('./pages/admin/export-hub/export-hub.component').then(m => m.ExportHubComponent) }
    ]
  },
  {
    path: 'landing',
    loadComponent: () => import('./pages/landing/landing.component').then(m => m.LandingPageComponent)
  },
  {
    path: 'errors',
    children: [
      { path: 'access-denied', loadComponent: () => import('./pages/errors/access-denied/access-denied.component').then(m => m.AccessDeniedComponent) },
      { path: 'not-found', loadComponent: () => import('./pages/errors/not-found/not-found.component').then(m => m.NotFoundComponent) }
    ]
  },
  { path: '', redirectTo: 'landing', pathMatch: 'full' },
  { path: '**', redirectTo: 'errors/not-found' }
];
