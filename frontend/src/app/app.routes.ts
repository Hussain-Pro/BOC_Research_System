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
    path: 'research',
    canActivate: [authGuard],
    children: [
      { path: 'timeline', loadComponent: () => import('./pages/research-timeline/research-timeline.component').then(m => m.ResearchTimelineComponent) },
      { path: 'submit', loadComponent: () => import('./pages/submit-research/submit-research.component').then(m => m.SubmitResearchComponent) }
    ]
  },
  {
    path: 'meetings',
    canActivate: [authGuard],
    children: [
      { path: 'rsvp', loadComponent: () => import('./pages/rsvp/rsvp.component').then(m => m.RsvpComponent) },
      { path: 'studio', loadComponent: () => import('./pages/meeting-studio/meeting-studio.component').then(m => m.MeetingStudioComponent) }
    ]
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    children: [
      { path: 'notifications', loadComponent: () => import('./pages/notifications/notifications.component').then(m => m.NotificationsComponent) },
      { path: 'chat', loadComponent: () => import('./pages/chat/chat.component').then(m => m.ChatComponent) },
      { path: 'analytics', loadComponent: () => import('./pages/admin/analytics-dashboard/analytics-dashboard.component').then(m => m.AnalyticsDashboardComponent) },
      { path: 'roster', loadComponent: () => import('./pages/admin/evaluator-roster/evaluator-roster.component').then(m => m.EvaluatorRosterComponent) },
      { path: 'ministry', loadComponent: () => import('./pages/admin/ministry-gateway/ministry-gateway.component').then(m => m.MinistryGatewayComponent) },
      { path: 'audit', loadComponent: () => import('./pages/admin/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent) },
      { path: 'config', loadComponent: () => import('./pages/admin/system-config/system-config.component').then(m => m.SystemConfigComponent) }
    ]
  },
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  { path: '**', redirectTo: 'auth/login' }
];
