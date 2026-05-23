import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-evaluator-portfolio',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './evaluator-portfolio.component.html',
  styleUrls: ['./evaluator-portfolio.component.scss']
})
export class EvaluatorPortfolioComponent implements OnInit {

  // Mock data representing the Evaluator's history
  portfolioRecords = [
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
      delayDays: 4, // SLA breached by 4 days
      committeeFinalStatus: 'Fail_Rejected'
    }
  ];

  kpis = {
    totalEvaluated: 45,
    averageScoreGiven: 72.5,
    onTimePercentage: 95
  };

  constructor() { }

  ngOnInit(): void {
  }

}
