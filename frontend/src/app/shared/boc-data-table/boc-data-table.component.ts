import { Component, Input, ViewChild, AfterViewInit, OnChanges, ContentChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { BocTableCellDefDirective } from './boc-table-cell.directive';

@Component({
  selector: 'boc-data-table',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule],
  templateUrl: './boc-data-table.component.html',
  styleUrl: './boc-data-table.component.scss'
})
export class BocDataTableComponent implements AfterViewInit, OnChanges {
  @Input() columns: string[] = [];
  @Input() columnLabels: Record<string, string> = {};
  @Input() data: unknown[] = [];
  @Input() pageSize = 10;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ContentChildren(BocTableCellDefDirective) cellDefs!: QueryList<BocTableCellDefDirective>;

  dataSource = new MatTableDataSource<unknown>([]);

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    this.dataSource.data = this.data;
  }

  ngOnChanges(): void {
    if (this.dataSource) {
      this.dataSource.data = this.data;
    }
  }

  labelFor(col: string): string {
    return this.columnLabels[col] ?? col;
  }

  cellDefFor(col: string): BocTableCellDefDirective | undefined {
    return this.cellDefs?.find(def => def.column === col);
  }
}
