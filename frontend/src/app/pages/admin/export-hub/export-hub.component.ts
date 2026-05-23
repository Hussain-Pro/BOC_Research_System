import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-export-hub',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './export-hub.component.html',
  styleUrls: ['./export-hub.component.scss']
})
export class ExportHubComponent implements OnInit {

  isExporting = false;

  reports = [
    { title: 'تقرير الأبحاث المنجزة (شهري)', type: 'PDF', icon: 'bi-file-pdf', color: 'text-danger' },
    { title: 'كشف مالي بمكافآت التقييم', type: 'Excel', icon: 'bi-file-excel', color: 'text-success' },
    { title: 'سجل التزام المقيمين بالوقت (SLA)', type: 'Excel', icon: 'bi-file-excel', color: 'text-success' },
    { title: 'محاضر اللجان المصادق عليها', type: 'Word', icon: 'bi-file-word', color: 'text-primary' }
  ];

  constructor(private toastService: ToastService) { }

  ngOnInit(): void {
  }

  generateReport(report: any) {
    this.isExporting = true;
    setTimeout(() => {
      this.isExporting = false;
      this.toastService.success(`تم استخراج ${report.title} بنجاح. جارٍ التحميل...`);
    }, 1500);
  }
}
