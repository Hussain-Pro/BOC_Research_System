import { Component, inject, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { OnboardingService } from '../../services/onboarding.service';
import { BocLayoutService } from '../../services/boc-layout.service';
import { NgApexchartsModule } from 'ng-apexcharts';
import { BocGlassCardComponent } from '../../shared/boc-glass-card/boc-glass-card.component';
import { BocStatCardComponent } from '../../shared/boc-stat-card/boc-stat-card.component';

@Component({
  selector: 'app-home-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, NgApexchartsModule, BocGlassCardComponent, BocStatCardComponent],
  templateUrl: './home-dashboard.component.html',
  styleUrls: ['./home-dashboard.component.scss']
})
export class HomeDashboardComponent implements OnInit, AfterViewInit {
  authService = inject(AuthService);
  onboardingService = inject(OnboardingService);
  private layoutService = inject(BocLayoutService);

  userRole = '';
  userName = '';
  submissionsChartOptions: Record<string, unknown>;

  quickActions = [
    { route: '/research/submit', icon: 'bi-file-earmark-plus', label: 'تقديم بحث جديد', class: 'btn-primary', roles: ['Researcher', 'Admin', 'Chairman', 'Member', 'Evaluator', 'Secretary', 'Supervisor', 'Clerk', 'Deputy'] },
    { route: '/research/history', icon: 'bi-clock-history', label: 'متابعة مسار البحث', class: 'btn-danger', roles: ['Researcher', 'Admin', 'Chairman', 'Member', 'Evaluator', 'Secretary', 'Supervisor', 'Clerk', 'Deputy'] },
    { route: '/triage', icon: 'bi-funnel', label: 'فرز الأبحاث الواردة', style: 'var(--boc-accent-triage)', roles: ['Chairman', 'Supervisor', 'Admin'] },
    { route: '/committee/workspace', icon: 'bi-clipboard-check', label: 'مساحة عمل المقيم', style: 'var(--boc-accent-evaluator)', roles: ['Evaluator', 'Member', 'Chairman', 'Admin'] },
    { route: '/committee/scheduler', icon: 'bi-journal-text', label: 'جدولة الاجتماعات', style: 'var(--boc-accent-committee)', roles: ['Secretary', 'Chairman', 'Admin'] },
    { route: '/profile', icon: 'bi-person', label: 'تحديث الملف الشخصي', class: 'btn-light border', roles: ['Researcher', 'Admin', 'Chairman', 'Member', 'Evaluator', 'Secretary', 'Supervisor', 'Clerk', 'Deputy'] }
  ];

  adminCards = [
    { route: '/admin/search', icon: 'bi-search', title: 'محرك البحث الشامل', link: 'الذهاب للبحث', color: 'var(--boc-primary)' },
    { route: '/admin/export', icon: 'bi-file-earmark-arrow-down', title: 'مركز تصدير التقارير', link: 'تصدير الآن', color: 'var(--boc-accent-export)' },
    { route: '/admin/violations', icon: 'bi-alarm', title: 'لوحة الأبحاث المتأخرة (SLA)', link: 'عرض المخالفات', color: 'var(--boc-danger)' },
    { route: '/admin/plagiarism', icon: 'bi-shield-exclamation', title: 'تجاوز حظر الانتحال', link: 'مراجعة الحظر', color: 'var(--boc-accent-plagiarism)' },
    { route: '/admin/roster', icon: 'bi-people', title: 'سجل المقيمين', link: 'إدارة المقيمين', color: 'var(--boc-info)' },
    { route: '/admin/ministry', icon: 'bi-bank', title: 'بوابة تصدير الوزارة', link: 'فتح البوابة', color: 'var(--boc-accent-ministry)' },
    { route: '/admin/config', icon: 'bi-gear', title: 'إعدادات النظام العامة', link: 'الذهاب للإعدادات', color: 'var(--boc-secondary)' }
  ];

  constructor() {
    this.submissionsChartOptions = {
      series: [{ name: 'البحوث المقدمة', data: [1, 2, 4, 3, 5, 8, 12] }],
      chart: { height: 300, type: 'area', fontFamily: 'Tajawal, sans-serif', toolbar: { show: false } },
      colors: ['#0F2A38'],
      dataLabels: { enabled: false },
      stroke: { curve: 'smooth' },
      xaxis: { categories: ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو'] },
      grid: { borderColor: 'rgba(74,96,122,0.15)' }
    };
  }

  ngOnInit() {
    this.layoutService.setPage('لوحة التحكم', [{ label: 'الرئيسية' }]);
    this.userRole = this.authService.getRole();
    const user = this.authService.currentUser();
    this.userName = user?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'مستخدم النظام';
  }

  ngAfterViewInit() {
    this.onboardingService.startHomeTour(this.userRole);
  }

  startTour() {
    this.onboardingService.startHomeTour(this.userRole, true);
  }

  visibleActions() {
    return this.quickActions.filter(a => !a.roles || a.roles.includes(this.userRole) || this.userRole === 'Admin');
  }
}
