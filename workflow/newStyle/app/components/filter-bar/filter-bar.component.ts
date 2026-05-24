import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

export interface FilterChip {
  key: string;
  label: string;
  value: any;
}

@Component({
  selector: 'app-filter-bar',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './filter-bar.component.html',
  styleUrls: ['./filter-bar.component.scss']
})
export class FilterBarComponent {
  @Input() activeFilters: FilterChip[] = [];
  @Output() search = new EventEmitter<string>();
  @Output() filterRemove = new EventEmitter<FilterChip>();
  @Output() filterClear = new EventEmitter<void>();
  @Output() filterDrawerOpen = new EventEmitter<void>();

  searchQuery = '';

  onSearch(): void {
    this.search.emit(this.searchQuery);
  }

  removeFilter(filter: FilterChip): void {
    this.filterRemove.emit(filter);
  }

  clearAll(): void {
    this.searchQuery = '';
    this.filterClear.emit();
  }

  openFilterDrawer(): void {
    this.filterDrawerOpen.emit();
  }
}
