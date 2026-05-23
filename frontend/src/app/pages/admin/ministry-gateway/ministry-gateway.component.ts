import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-ministry-gateway',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './ministry-gateway.component.html',
  styleUrls: ['./ministry-gateway.component.scss']
})
export class MinistryGatewayComponent implements OnInit {

  batches = [
    { id: 'MIN-BATCH-2026-05A', sentDate: '2026-05-15T10:30:00Z', count: 5, status: 'Received_Success' },
    { id: 'MIN-BATCH-2026-04B', sentDate: '2026-04-28T11:00:00Z', count: 12, status: 'Received_Success' },
    { id: 'MIN-BATCH-2026-06A', sentDate: '2026-06-01T09:00:00Z', count: 3, status: 'Pending' }
  ];

  isSending = false;

  constructor(private toastService: ToastService) { }

  ngOnInit(): void {
  }

  sendNewBatch() {
    this.isSending = true;
    setTimeout(() => {
      this.isSending = false;
      const newBatch = {
        id: `MIN-BATCH-${new Date().toISOString().substring(0,7).replace('-','')}-NEW`,
        sentDate: new Date().toISOString(),
        count: 7, // Simulated 7 new passed researches
        status: 'Pending'
      };
      this.batches.unshift(newBatch);
      this.toastService.success('تم إرسال الدفعة الجديدة للوزارة (API) بنجاح. حالة الدفعة قيد الانتظار.');
    }, 2000);
  }
}
