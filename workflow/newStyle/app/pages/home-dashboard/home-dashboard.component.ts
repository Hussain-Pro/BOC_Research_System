import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { NgApexchartsModule, ChartComponent } from 'ng-apexcharts';
import { PageHeaderComponent, PageAction } from '../../components/page-header/page-header.component';
import { DataTableComponent, TableColumn, TableRowAction } from '../../components/data-table/data-table.component';
import { ToastService } from '../../components/toast-system/toast.service';

interface KpiData {
  icon: string;
  iconBg: string;
  iconColor: string;
  value: number;
  label: string;
  trend: number;
  trendText: string;
}

interface ActivityItem {
  initials: string;
  actor: string;
  action: string;
  time: string;
}

interface QuickAction {
  icon: string;
  label: string;
  desc: string;
  bg: string;
  color: string;
  handler: () => void;
}

@Component({
  selector: 'app-home-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    NgApexchartsModule,
    PageHeaderComponent,
    DataTableComponent
  ],
  templateUrl: './home-dashboard.component.html',
  styleUrls: ['./home-dashboard.component.scss']
})
export class HomeDashboardComponent implements OnInit {
  @ViewChild('chart') chart!: ChartComponent;

  userName = 'أحمد';

  pageActions: PageAction[] = [
    {
      label: 'COMMON.EXPORT',
      variant: 'secondary',
      icon: 'bi bi-download',
      handler: () => this.exportDashboard()
    },
    {
      label: 'RESEARCH.SUBMIT_NEW',
      variant: 'primary',
      icon: 'bi bi-plus-lg',
      handler: () => this.router.navigate(['/submit-research'])
    }
  ];

  kpis: KpiData[] = [
    { icon: 'bi bi-file-earmark-text', iconBg: 'rgba(15,42,56,0.08)', iconColor: 'var(--boc-primary)', value: 128, label: 'DASHBOARD.KPI_TOTAL', trend: 12, trendText: 'DASHBOARD.KPI_TOTAL_TREND' },
    { icon: 'bi bi-hourglass-split', iconBg: 'rgba(227,116,0,0.10)', iconColor: 'var(--boc-accent-triage)', value: 24, label: 'DASHBOARD.KPI_PENDING', trend: 3, trendText: 'DASHBOARD.KPI_PENDING_TREND' },
    { icon: 'bi bi-check-circle', iconBg: 'rgba(40,167,69,0.10)', iconColor: 'var(--boc-success)', value: 86, label: 'DASHBOARD.KPI_APPROVED', trend: -2, trendText: 'DASHBOARD.KPI_APPROVED_TREND' },
    { icon: 'bi bi-people', iconBg: 'rgba(132,48,206,0.10)', iconColor: 'var(--boc-accent-evaluator)', value: 7, label: 'DASHBOARD.KPI_MEETINGS', trend: 2, trendText: 'DASHBOARD.KPI_MEETINGS_TREND' }
  ];

  chartPeriods = [
    { value: 'daily', label: 'COMMON.DAILY' },
    { value: 'weekly', label: 'COMMON.WEEKLY' },
    { value: 'monthly', label: 'COMMON.MONTHLY' },
    { value: 'yearly', label: 'COMMON.YEARLY' }
  ];
  selectedPeriod = 'monthly';

  chartSeries = [44, 55, 41, 17, 15];
  chartLabels = ['مسودة', 'قيد المراجعة', 'تمت الموافقة', 'مرفوض', 'يحتاج تصحيح'];
  chartColors = ['#4A607A', '#17a2b8', '#28a745', '#dc3545', '#ffc107'];
  chartOptions: any = {
    chart: {
      type: 'donut',
      height: 320,
      fontFamily: 'Tajawal, sans-serif',
      animations: {
        enabled: true,
        easing: 'easeinout',
        speed: 800
      }
    },
    plotOptions: {
      pie: {
        donut: {
          size: '65%',
          labels: {
            show: true,
            name: { show: true, fontSize: '14px', fontFamily: 'Tajawal' },
            value: { show: true, fontSize: '22px', fontWeight: 700, fontFamily: 'Tajawal' },
            total: {
              show: true,
              label: 'الإجمالي',
              fontSize: '14px',
              fontFamily: 'Tajawal',
              formatter: (w: any) => w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0)
            }
          }
        }
      }
    },
    dataLabels: { enabled: false },
    legend: {
      position: 'bottom',
      fontFamily: 'Tajawal',
      fontSize: '13px',
      markers: { width: 10, height: 10, radius: 3 }
    },
    tooltip: {
      theme: 'light',
      style: { fontFamily: 'Tajawal' }
    },
    stroke: { show: true, width: 2, colors: ['var(--boc-surface)'] }
  };

  recentActivity: ActivityItem[] = [
    { initials: 'مع', actor: 'محمود العلي', action: 'DASHBOARD.ACT_SUBMIT_RESEARCH', time: 'COMMON.MINUTES_AGO_15' },
    { initials: 'فز', actor: 'فاطمة الزهراء', action: 'DASHBOARD.ACT_CORRECTION', time: 'COMMON.MINUTES_AGO_45' },
    { initials: 'سع', actor: 'سعيد المنصوري', action: 'DASHBOARD.ACT_APPROVE', time: 'COMMON.HOURS_AGO_2' },
    { initials: 'نخ', actor: 'نورة الخالدي', action: 'DASHBOARD.ACT_SCHEDULE_MEETING', time: 'COMMON.HOURS_AGO_5' },
    { initials: 'أح', actor: 'أحمد الخالدي', action: 'DASHBOARD.ACT_COMMENT', time: 'COMMON.YESTERDAY' }
  ];

  reviewColumns: TableColumn[] = [
    { key: 'title', header: 'TABLE.TITLE', type: 'text', emphasize: true },
    { key: 'researcher', header: 'TABLE.RESEARCHER', type: 'avatar' },
    { key: 'status', header: 'TABLE.STATUS', type: 'status' },
    { key: 'role', header: 'TABLE.ROLE', type: 'status' },
    { key: 'date', header: 'TABLE.DATE', type: 'date' }
  ];

  reviewActions: TableRowAction[] = [
    { icon: 'bi bi-eye', label: 'COMMON.VIEW', handler: (row) => this.viewResearch(row) },
    { icon: 'bi bi-pencil', label: 'COMMON.EDIT', handler: (row) => this.editResearch(row) },
    { icon: 'bi bi-three-dots-vertical', label: 'COMMON.MORE', handler: (row) => this.moreActions(row) }
  ];

  pendingReviews = [
    { title: 'تقييم أثر التضخم على القطاع المصرفي', researcher: 'محمود العلي', status: 'review', role: 'triage', date: new Date('2026-05-24') },
    { title: 'دراسة السيولة في الأسواق الناشئة', researcher: 'فاطمة الزهراء', status: 'draft', role: 'evaluator', date: new Date('2026-05-22') },
    { title: 'تحليل المخاطر الائتمانية لعام 2026', researcher: 'سعيد المنصوري', status: 'success', role: 'committee', date: new Date('2026-05-18') },
    { title: 'تقرير الاستقرار المالي النصف سنوي', researcher: 'نورة الخالدي', status: 'warning', role: 'triage', date: new Date('2026-05-15') }
  ];

  quickActions: QuickAction[] = [
    { icon: 'bi bi-file-earmark-plus', label: 'QUICK.SUBMIT', desc: 'QUICK.SUBMIT_DESC', bg: 'rgba(15,42,56,0.08)', color: 'var(--boc-primary)', handler: () => this.router.navigate(['/submit-research']) },
    { icon: 'bi bi-calendar-plus', label: 'QUICK.SCHEDULE', desc: 'QUICK.SCHEDULE_DESC', bg: 'rgba(13,110,253,0.10)', color: 'var(--boc-accent-committee)', handler: () => this.router.navigate(['/meeting-scheduler']) },
    { icon: 'bi bi-clipboard-check', label: 'QUICK.REVIEW', desc: 'QUICK.REVIEW_DESC', bg: 'rgba(227,116,0,0.10)', color: 'var(--boc-accent-triage)', handler: () => this.router.navigate(['/triage-dashboard']) },
    { icon: 'bi bi-chat-square-text', label: 'QUICK.MESSAGE', desc: 'QUICK.MESSAGE_DESC', bg: 'rgba(132,48,206,0.10)', color: 'var(--boc-accent-evaluator)', handler: () => this.router.navigate(['/chat']) }
  ];

  constructor(
    private router: Router,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    // Load dashboard data from service
  }

  setChartPeriod(period: string): void {
    this.selectedPeriod = period;
    // Refresh chart data
  }

  navigateToTasks(): void {
    this.router.navigate(['/triage-dashboard']);
  }

  navigateToTriage(): void {
    this.router.navigate(['/triage-dashboard']);
  }

  viewAllActivity(): void {
    this.router.navigate(['/notifications']);
  }

  exportDashboard(): void {
    this.toastService.show('DASHBOARD.EXPORT_STARTED', 'info');
    // Export logic
  }

  viewResearch(row: any): void {
    this.router.navigate(['/research-timeline', row.id || 1]);
  }

  editResearch(row: any): void {
    this.router.navigate(['/submit-research', row.id || 1]);
  }

  moreActions(row: any): void {
    // Open action menu
  }
}
