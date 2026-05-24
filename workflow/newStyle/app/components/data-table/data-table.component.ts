import { Component, Input, Output, EventEmitter, TemplateRef, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

export interface TableColumn {
  key: string;
  header: string;
  type?: 'text' | 'status' | 'date' | 'avatar' | 'custom';
  width?: string;
  align?: 'left' | 'right' | 'center';
  sortable?: boolean;
  emphasize?: boolean;
  template?: TemplateRef<any>;
}

export interface TableRowAction {
  icon: string;
  label: string;
  handler: (row: any) => void;
}

export interface ViewMode {
  value: string;
  icon: string;
  label: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss']
})
export class DataTableComponent implements OnChanges {
  @Input() data: any[] = [];
  @Input() columns: TableColumn[] = [];
  @Input() rowActions: TableRowAction[] = [];
  @Input() selectable = false;
  @Input() showToolbar = true;
  @Input() showPagination = true;
  @Input() allowExport = false;
  @Input() pageSize = 10;
  @Input() viewModes: ViewMode[] = [];
  @Input() loading = false;

  @Output() selectionChange = new EventEmitter<any[]>();
  @Output() sortChange = new EventEmitter<{ column: string; direction: 'asc' | 'desc' }>();
  @Output() pageChange = new EventEmitter<number>();

  searchQuery = '';
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  currentPage = 1;
  selectedRows: any[] = [];
  currentViewMode = 'list';
  filteredData: any[] = [];

  get paginatedData(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredData.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredData.length / this.pageSize) || 1;
  }

  get startIndex(): number {
    return (this.currentPage - 1) * this.pageSize;
  }

  get endIndex(): number {
    return Math.min(this.startIndex + this.pageSize, this.filteredData.length);
  }

  get isAllSelected(): boolean {
    return this.paginatedData.length > 0 && this.paginatedData.every(row => this.isSelected(row));
  }

  get hasActiveFilters(): boolean {
    return this.searchQuery.length > 0;
  }

  get visiblePages(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data']) {
      this.filteredData = [...this.data];
      this.applySort();
    }
  }

  onSearch(): void {
    if (!this.searchQuery.trim()) {
      this.filteredData = [...this.data];
    } else {
      const q = this.searchQuery.toLowerCase();
      this.filteredData = this.data.filter(row =>
        this.columns.some(col => {
          const val = this.getCellValue(row, col);
          return val != null && String(val).toLowerCase().includes(q);
        })
      );
    }
    this.currentPage = 1;
    this.applySort();
  }

  sort(columnKey: string): void {
    if (this.sortColumn === columnKey) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = columnKey;
      this.sortDirection = 'asc';
    }
    this.applySort();
    this.sortChange.emit({ column: this.sortColumn, direction: this.sortDirection });
  }

  private applySort(): void {
    if (!this.sortColumn) return;

    this.filteredData.sort((a, b) => {
      const aVal = this.getNestedValue(a, this.sortColumn);
      const bVal = this.getNestedValue(b, this.sortColumn);

      if (aVal == null) return this.sortDirection === 'asc' ? -1 : 1;
      if (bVal == null) return this.sortDirection === 'asc' ? 1 : -1;

      if (typeof aVal === 'string') {
        return this.sortDirection === 'asc'
          ? aVal.localeCompare(bVal)
          : bVal.localeCompare(aVal);
      }

      return this.sortDirection === 'asc'
        ? (aVal < bVal ? -1 : 1)
        : (aVal > bVal ? -1 : 1);
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.pageChange.emit(page);
  }

  toggleSelectAll(): void {
    if (this.isAllSelected) {
      this.selectedRows = this.selectedRows.filter(
        r => !this.paginatedData.some(pr => pr === r)
      );
    } else {
      this.paginatedData.forEach(row => {
        if (!this.isSelected(row)) this.selectedRows.push(row);
      });
    }
    this.selectionChange.emit([...this.selectedRows]);
  }

  toggleRow(row: any): void {
    const index = this.selectedRows.indexOf(row);
    if (index >= 0) {
      this.selectedRows.splice(index, 1);
    } else {
      this.selectedRows.push(row);
    }
    this.selectionChange.emit([...this.selectedRows]);
  }

  isSelected(row: any): boolean {
    return this.selectedRows.includes(row);
  }

  getCellValue(row: any, col: TableColumn): any {
    return this.getNestedValue(row, col.key);
  }

  private getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((o, p) => o?.[p], obj);
  }

  getAvatarInitials(name: string): string {
    if (!name) return '?';
    return name.split(' ').map(n => n[0]).join('').slice(0, 2);
  }

  toggleFilters(): void {
    // Open filter drawer or expand filter bar
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.filteredData = [...this.data];
    this.currentPage = 1;
  }

  setViewMode(mode: string): void {
    this.currentViewMode = mode;
  }

  exportData(): void {
    // CSV/Excel export logic
  }
}
