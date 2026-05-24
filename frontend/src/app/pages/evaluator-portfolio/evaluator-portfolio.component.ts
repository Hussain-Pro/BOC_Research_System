import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BocLayoutService } from '../../services/boc-layout.service';
import {
  BocDataTableComponent
} from '../../shared';

interface PortfolioRecord {
  id: string;
  researchId: string;
  title: string;
  assignedDate: string;
  evaluatedDate: string;
  givenScore: number;
  delayDays: number;
  committeeFinalStatus: string;
}

@Component({
  selector: 'app-evaluator-portfolio',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    BocDataTableComponent
  ],
  templateUrl: './evaluator-portfolio.component.html',
  styleUrls: ['./evaluator-portfolio.component.scss']
})
export class EvaluatorPortfolioComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  portfolioRecords: PortfolioRecord[] = [
    {
      id: 'A-0091',
      researchId: 'R-09511',
      title: 'تحسين كفاءة الحفر باستخدام تقنيات نانوية',
      assignedDate: '2025-01-10',
      evaluatedDate: '2025-01-18',
      givenScore: 85,
      delayDays: 0,
      committeeFinalStatus: 'Pass_Approved'
    },
    {
      id: 'A-0050',
      researchId: 'R-09120',
      title: 'تقييم المخاطر البيئية للتسربات النفطية',
      assignedDate: '2024-09-01',
      evaluatedDate: '2024-09-15',
      givenScore: 50,
      delayDays: 4,
      committeeFinalStatus: 'Fail_Rejected'
    }
  ];

  kpis = {
    totalEvaluated: 45,
    averageScoreGiven: 72.5,
    onTimePercentage: 95
  };

  tableColumns = ['id', 'title', 'assignedDate', 'evaluatedDate', 'givenScore', 'delayLabel', 'decisionLabel'];
  tableLabels: Record<string, string> = {
    id: 'رقم التقييم',
    title: 'عنوان البحث',
    assignedDate: 'تاريخ الإسناد',
    evaluatedDate: 'تاريخ الإنجاز',
    givenScore: 'الدرجة الممنوحة',
    delayLabel: 'التأخير',
    decisionLabel: 'القرار النهائي'
  };

  tableData: Record<string, string | number>[] = [];

  breadcrumbs = [
    { label: 'الرئيسية', route: '/home' },
    { label: 'السجل التاريخي للمقيّم' }
  ];

  ngOnInit(): void {
    this.layoutService.setPage('السجل التاريخي للمقيّم');
    this.tableData = this.portfolioRecords.map(record => ({
      id: record.id,
      title: `${record.researchId} — ${record.title}`,
      assignedDate: this.formatDate(record.assignedDate),
      evaluatedDate: this.formatDate(record.evaluatedDate),
      givenScore: `${record.givenScore}%`,
      delayLabel: record.delayDays > 0 ? `${record.delayDays} أيام` : 'ملتزم بالموعد',
      decisionLabel: record.committeeFinalStatus === 'Pass_Approved' ? 'مقبول نهائياً' : 'مرفوض نهائياً'
    }));
  }

  private formatDate(value: string): string {
    return new Date(value).toLocaleDateString('ar-EG');
  }
}
