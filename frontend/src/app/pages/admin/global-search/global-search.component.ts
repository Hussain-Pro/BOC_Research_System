import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BocLayoutService } from '../../../services/boc-layout.service';

@Component({
  selector: 'app-global-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule],
  templateUrl: './global-search.component.html',
  styleUrls: ['./global-search.component.scss']
})
export class GlobalSearchComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  searchQuery = '';
  selectedFilter = 'all';

  searchResults = [
    { id: 'R-10045', title: 'تأثير الضخ العميق', type: 'Research', status: 'Non_Compliant_Returned', date: '2026-05-20' },
    { id: 'EMP-901', title: 'د. أحمد حسين', type: 'Evaluator', status: 'Active', date: '2022-01-15' },
    { id: 'MIN-004', title: 'محضر اجتماع رقم 4', type: 'Meeting', status: 'Frozen', date: '2026-05-18' }
  ];

  constructor() { }

  ngOnInit(): void {
    this.layoutService.setPage('محرك البحث الشامل', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'البحث الشامل' }
    ]);
  }

  performSearch() {
    // In a real app, this would hit an API endpoint with query and filter
  }

  getTypeLabel(type: string): string {
    switch (type) {
      case 'Research': return 'بحث';
      case 'Evaluator': return 'مقيّم';
      case 'Meeting': return 'محضر اجتماع';
      default: return type;
    }
  }

  getTypeIcon(type: string): string {
    switch (type) {
      case 'Research': return 'bi-file-earmark-text';
      case 'Evaluator': return 'bi-person-badge';
      case 'Meeting': return 'bi-journal-text';
      default: return 'bi-search';
    }
  }

  getTypeBadgeClass(type: string): string {
    switch (type) {
      case 'Research': return 'badge-research';
      case 'Evaluator': return 'badge-evaluator';
      case 'Meeting': return 'badge-meeting';
      default: return 'bg-secondary';
    }
  }
}

