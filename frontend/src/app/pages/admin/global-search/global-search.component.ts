import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-global-search',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './global-search.component.html',
  styleUrls: ['./global-search.component.scss']
})
export class GlobalSearchComponent implements OnInit {

  searchQuery = '';
  selectedFilter = 'all';

  searchResults = [
    { id: 'R-10045', title: 'تأثير الضخ العميق', type: 'Research', status: 'Non_Compliant_Returned', date: '2026-05-20' },
    { id: 'EMP-901', title: 'د. أحمد حسين', type: 'Evaluator', status: 'Active', date: '2022-01-15' },
    { id: 'MIN-004', title: 'محضر اجتماع رقم 4', type: 'Meeting', status: 'Frozen', date: '2026-05-18' }
  ];

  constructor() { }

  ngOnInit(): void {
  }

  performSearch() {
    // In a real app, this would hit an API endpoint with query and filter
  }
}
