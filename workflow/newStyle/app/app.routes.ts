import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { publicGuard } from './core/guards/public.guard';

export const routes: Routes = [
  // Public routes (no shell)
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent),
    canActivate: [publicGuard]
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent),
    canActivate: [publicGuard]
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./pages/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent),
    canActivate: [publicGuard]
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./pages/reset-password/reset-password.component').then(m => m.ResetPasswordComponent),
    canActivate: [publicGuard]
  },
  {
    path: 'two-factor',
    loadComponent: () => import('./pages/two-factor/two-factor.component').then(m => m.TwoFactorComponent),
    canActivate: [publicGuard]
  },
  {
    path: 'landing',
    loadComponent: () => import('./pages/landing/landing.component').then(m => m.LandingComponent)
  },

  // Protected routes (inside shell)
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./pages/home-dashboard/home-dashboard.component').then(m => m.HomeDashboardComponent) },
      { path: 'submit-research', loadComponent: () => import('./pages/submit-research/submit-research.component').then(m => m.SubmitResearchComponent) },
      { path: 'researcher-history', loadComponent: () => import('./pages/researcher-history/researcher-history.component').then(m => m.ResearcherHistoryComponent) },
      { path: 'research-corrections', loadComponent: () => import('./pages/research-corrections/research-corrections.component').then(m => m.ResearchCorrectionsComponent) },
      { path: 'research-timeline/:id', loadComponent: () => import('./pages/research-timeline/research-timeline.component').then(m => m.ResearchTimelineComponent) },
      { path: 'committee-workspace', loadComponent: () => import('./pages/committee-workspace/committee-workspace.component').then(m => m.CommitteeWorkspaceComponent) },
      { path: 'meeting-scheduler', loadComponent: () => import('./pages/meeting-scheduler/meeting-scheduler.component').then(m => m.MeetingSchedulerComponent) },
      { path: 'meeting-studio', loadComponent: () => import('./pages/meeting-studio/meeting-studio.component').then(m => m.MeetingStudioComponent) },
      { path: 'triage-dashboard', loadComponent: () => import('./pages/triage-dashboard/triage-dashboard.component').then(m => m.TriageDashboardComponent) },
      { path: 'evaluator-portfolio', loadComponent: () => import('./pages/evaluator-portfolio/evaluator-portfolio.component').then(m => m.EvaluatorPortfolioComponent) },
      { path: 'chat', loadComponent: () => import('./pages/chat/chat.component').then(m => m.ChatComponent) },
      { path: 'notifications', loadComponent: () => import('./pages/notifications/notifications.component').then(m => m.NotificationsComponent) },
      { path: 'user-profile', loadComponent: () => import('./pages/user-profile/user-profile.component').then(m => m.UserProfileComponent) },
      { path: 'admin', loadComponent: () => import('./pages/admin/admin.component').then(m => m.AdminComponent) },
      { path: 'rsvp', loadComponent: () => import('./pages/rsvp/rsvp.component').then(m => m.RsvpComponent) }
    ]
  },

  // Error pages
  {
    path: 'error/:code',
    loadComponent: () => import('./pages/errors/error.component').then(m => m.ErrorComponent)
  },

  // Fallback
  { path: '**', redirectTo: 'landing' }
];
