import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-sla-violations',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sla-violations.component.html',
  styleUrls: ['./sla-violations.component.scss']
})
export class SlaViolationsComponent implements OnInit {

  violations = [
    {
      researchId: 'R-09120',
      researchTitle: 'تقييم المخاطر البيئية',
      evaluatorName: 'د. أحمد حسين',
      assignedDate: '2026-05-01',
      dueDate: '2026-05-11',
      delayDays: 13,
      status: 'Critical'
    },
    {
      researchId: 'R-09550',
      researchTitle: 'تحسين كفاءة الحفر',
      evaluatorName: 'م. سارة علي',
      assignedDate: '2026-05-10',
      dueDate: '2026-05-20',
      delayDays: 4,
      status: 'Warning'
    }
  ];

  constructor(private toastService: ToastService) { }

  ngOnInit(): void {
  }

  sendReminder(evaluator: string) {
    this.toastService.success(`تم إرسال تذكير عاجل إلى المقيّم: ${evaluator}`);
  }

  reassignEvaluation(researchId: string) {
    this.toastService.success(`تم سحب البحث ${researchId} وسيتم إعادته لشاشة الفرز (Triage).`);
    this.violations = this.violations.filter(v => v.researchId !== researchId);
  }
}
