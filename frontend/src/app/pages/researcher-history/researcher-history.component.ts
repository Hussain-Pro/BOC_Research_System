import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-researcher-history',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './researcher-history.component.html',
  styleUrls: ['./researcher-history.component.scss']
})
export class ResearcherHistoryComponent implements OnInit {

  historyRecords = [
    { id: 'R-09820', title: 'تأثير درجة الحرارة على كثافة النفط الخام', category: 'هندسة مكامن', submittedAt: '2026-03-10', status: 'Accepted', score: 85 },
    { id: 'R-09950', title: 'تحليل البيانات السيزمية ثنائية الأبعاد', category: 'جيولوجيا', submittedAt: '2026-04-15', status: 'Non_Compliant_Returned', score: null },
    { id: 'R-10045', title: 'تقييم كفاءة موائع الحفر الذكية', category: 'هندسة حفر', submittedAt: '2026-05-20', status: 'Pending_Triage', score: null },
    { id: 'R-09120', title: 'التكسير الهيدروليكي في الآبار المتقادمة', category: 'هندسة إنتاج', submittedAt: '2025-11-05', status: 'Rejected', score: 45 }
  ];

  constructor() { }

  ngOnInit(): void {
  }

  getStatusBadge(status: string) {
    switch (status) {
      case 'Accepted': return { label: 'مقبول نهائياً', class: 'bg-success' };
      case 'Rejected': return { label: 'مرفوض', class: 'bg-danger' };
      case 'Non_Compliant_Returned': return { label: 'معاد للتعديل', class: 'bg-warning text-dark border border-warning' };
      case 'Pending_Triage': return { label: 'قيد الفرز', class: 'bg-secondary' };
      case 'Under_Evaluation': return { label: 'قيد التقييم', class: 'bg-info text-dark' };
      default: return { label: status, class: 'bg-light text-dark' };
    }
  }
}
