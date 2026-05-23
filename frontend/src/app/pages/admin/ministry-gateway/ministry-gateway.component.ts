import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../../services/toast.service';
import { BocLayoutService } from '../../../services/boc-layout.service';
import { BocPageHeroComponent } from '../../../shared/boc-page-hero/boc-page-hero.component';
import { BocStatCardComponent } from '../../../shared/boc-stat-card/boc-stat-card.component';
import { BocGlassCardComponent } from '../../../shared/boc-glass-card/boc-glass-card.component';
import { BocDataTableComponent } from '../../../shared/boc-data-table/boc-data-table.component';
import { BocEmptyStateComponent } from '../../../shared/boc-empty-state/boc-empty-state.component';

@Component({
  selector: 'app-ministry-gateway',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    BocPageHeroComponent,
    BocStatCardComponent,
    BocGlassCardComponent,
    BocDataTableComponent,
    BocEmptyStateComponent
  ],
  templateUrl: './ministry-gateway.component.html',
  styleUrls: ['./ministry-gateway.component.scss']
})
export class MinistryGatewayComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  batches = [
    { id: 'MIN-BATCH-2026-05A', sentDate: '2026-05-15T10:30:00Z', count: 5, status: 'Received_Success' },
    { id: 'MIN-BATCH-2026-04B', sentDate: '2026-04-28T11:00:00Z', count: 12, status: 'Received_Success' },
    { id: 'MIN-BATCH-2026-06A', sentDate: '2026-06-01T09:00:00Z', count: 3, status: 'Pending' }
  ];

  isSending = false;

  tableColumns = ['id', 'sentDate', 'count', 'statusLabel'];
  columnLabels: Record<string, string> = {
    id: 'رقم الدفعة (Batch ID)',
    sentDate: 'تاريخ ووقت الإرسال',
    count: 'عدد الأبحاث المضمنة',
    statusLabel: 'حالة الاستلام بالوزارة'
  };

  constructor(private toastService: ToastService) { }

  ngOnInit(): void {
    this.layoutService.setPage('بوابة الوزارة', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'بوابة الوزارة' }
    ]);
  }

  sendNewBatch() {
    this.isSending = true;
    setTimeout(() => {
      this.isSending = false;
      const newBatch = {
        id: `MIN-BATCH-${new Date().toISOString().substring(0, 7).replace('-', '')}-NEW`,
        sentDate: new Date().toISOString(),
        count: 7,
        status: 'Pending'
      };
      this.batches.unshift(newBatch);
      this.toastService.success('تم إرسال الدفعة الجديدة للوزارة (API) بنجاح. حالة الدفعة قيد الانتظار.');
    }, 2000);
  }

  get tableData() {
    return this.batches.map(b => ({
      id: b.id,
      sentDate: new Date(b.sentDate).toLocaleString('ar-IQ'),
      count: b.count,
      statusLabel: b.status === 'Received_Success' ? 'تم الاستلام بنجاح' : 'قيد المعالجة بالوزارة'
    }));
  }

  get successCount(): number {
    return this.batches.filter(b => b.status === 'Received_Success').length;
  }

  get pendingCount(): number {
    return this.batches.filter(b => b.status === 'Pending').length;
  }
}
