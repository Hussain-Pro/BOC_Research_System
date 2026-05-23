import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BocLayoutService } from '../../../services/boc-layout.service';
import { BocPageHeroComponent } from '../../../shared/boc-page-hero/boc-page-hero.component';
import { BocStatCardComponent } from '../../../shared/boc-stat-card/boc-stat-card.component';
import { BocGlassCardComponent } from '../../../shared/boc-glass-card/boc-glass-card.component';
import { BocDataTableComponent } from '../../../shared/boc-data-table/boc-data-table.component';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    BocPageHeroComponent,
    BocStatCardComponent,
    BocGlassCardComponent,
    BocDataTableComponent
  ],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.scss']
})
export class AuditLogsComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  logs = [
    { date: '2026-05-23 09:12:44', user: 'د. محمود (EMP-1192)', action: 'UPDATE', entity: 'ResearchPaper' },
    { date: '2026-05-23 08:05:11', user: 'System Admin (EMP-0001)', action: 'LOGIN', entity: 'AppUser' }
  ];

  tableColumns = ['date', 'user', 'action', 'entity'];
  columnLabels: Record<string, string> = {
    date: 'التاريخ والوقت',
    user: 'المستخدم (ID)',
    action: 'الإجراء (Action)',
    entity: 'الكيان (Entity)'
  };

  constructor() { }

  ngOnInit(): void {
    this.layoutService.setPage('سجل التدقيق الأمني', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'سجل التدقيق' }
    ]);
  }
}
