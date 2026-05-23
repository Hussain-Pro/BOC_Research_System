import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.scss']
})
export class AuditLogsComponent implements OnInit {

  logs = [
    { date: '2026-05-23 09:12:44', user: 'د. محمود (EMP-1192)', action: 'UPDATE', entity: 'ResearchPaper' },
    { date: '2026-05-23 08:05:11', user: 'System Admin (EMP-0001)', action: 'LOGIN', entity: 'AppUser' }
  ];

  constructor() { }

  ngOnInit(): void {
  }
}
