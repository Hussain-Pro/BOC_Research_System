import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../../services/toast.service';
import { BocLayoutService } from '../../../services/boc-layout.service';
import { BocPageHeroComponent } from '../../../shared/boc-page-hero/boc-page-hero.component';
import { BocStatCardComponent } from '../../../shared/boc-stat-card/boc-stat-card.component';
import { BocGlassCardComponent } from '../../../shared/boc-glass-card/boc-glass-card.component';

@Component({
  selector: 'app-export-hub',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    BocPageHeroComponent,
    BocStatCardComponent,
    BocGlassCardComponent
  ],
  templateUrl: './export-hub.component.html',
  styleUrls: ['./export-hub.component.scss']
})
export class ExportHubComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  isExporting = false;

  reports = [
    { title: 'تقرير الأبحاث المنجزة (شهري)', type: 'PDF', icon: 'bi-file-pdf', colorClass: 'icon-pdf' },
    { title: 'كشف مالي بمكافآت التقييم', type: 'Excel', icon: 'bi-file-excel', colorClass: 'icon-excel' },
    { title: 'سجل التزام المقيمين بالوقت (SLA)', type: 'Excel', icon: 'bi-file-excel', colorClass: 'icon-excel' },
    { title: 'محاضر اللجان المصادق عليها', type: 'Word', icon: 'bi-file-word', colorClass: 'icon-word' }
  ];

  constructor(private toastService: ToastService) { }

  ngOnInit(): void {
    this.layoutService.setPage('مركز تصدير التقارير', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'مركز التصدير' }
    ]);
  }

  generateReport(report: { title: string }) {
    this.isExporting = true;
    setTimeout(() => {
      this.isExporting = false;
      this.toastService.success(`تم استخراج ${report.title} بنجاح. جارٍ التحميل...`);
    }, 1500);
  }
}
