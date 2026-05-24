import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BocLayoutService } from '../../services/boc-layout.service';
import { BocDataTableComponent } from '../../shared/boc-data-table/boc-data-table.component';
import { BocTableCellDefDirective } from '../../shared/boc-data-table/boc-table-cell.directive';
import { BocStatusChipComponent, BocStatusType } from '../../shared/boc-status-chip/boc-status-chip.component';

export interface ResearchHistoryRecord {
  id: string;
  title: string;
  category: string;
  submittedAt: string;
  status: string;
  score: number | null;
}

@Component({
  selector: 'app-researcher-history',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    BocDataTableComponent,
    BocTableCellDefDirective,
    BocStatusChipComponent
  ],
  templateUrl: './researcher-history.component.html',
  styleUrl: './researcher-history.component.scss'
})
export class ResearcherHistoryComponent implements OnInit, OnDestroy {
  private layoutService = inject(BocLayoutService);

  historyRecords: ResearchHistoryRecord[] = [
    { id: 'R-09820', title: 'تأثير درجة الحرارة على كثافة النفط الخام', category: 'هندسة مكامن', submittedAt: '2026-03-10', status: 'Accepted', score: 85 },
    { id: 'R-09950', title: 'تحليل البيانات السيزمية ثنائية الأبعاد', category: 'جيولوجيا', submittedAt: '2026-04-15', status: 'Non_Compliant_Returned', score: null },
    { id: 'R-10045', title: 'تقييم كفاءة موائع الحفر الذكية', category: 'هندسة حفر', submittedAt: '2026-05-20', status: 'Pending_Triage', score: null },
    { id: 'R-09120', title: 'التكسير الهيدروليكي في الآبار المتقادمة', category: 'هندسة إنتاج', submittedAt: '2025-11-05', status: 'Rejected', score: 45 }
  ];

  tableColumns = ['id', 'title', 'category', 'submittedAt', 'status', 'score', 'actions'];
  columnLabels: Record<string, string> = {
    id: 'رقم التتبع',
    title: 'عنوان البحث',
    category: 'التخصص / الفئة',
    submittedAt: 'تاريخ التقديم',
    status: 'الحالة النهائية',
    score: 'الدرجة',
    actions: 'الإجراءات'
  };

  ngOnInit(): void {
    this.layoutService.setPage('تاريخ الأبحاث الشخصي', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'تاريخ الأبحاث' }
    ]);
  }

  ngOnDestroy(): void {
    this.layoutService.clearPage();
  }

  getStatusChip(status: string): { label: string; type: BocStatusType } {
    switch (status) {
      case 'Accepted':
        return { label: 'مقبول نهائياً', type: 'success' };
      case 'Rejected':
        return { label: 'مرفوض', type: 'danger' };
      case 'Non_Compliant_Returned':
        return { label: 'معاد للتعديل', type: 'warning' };
      case 'Pending_Triage':
        return { label: 'قيد الفرز', type: 'draft' };
      case 'Under_Evaluation':
        return { label: 'قيد التقييم', type: 'review' };
      default:
        return { label: status, type: 'draft' };
    }
  }
}
