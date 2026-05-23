import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type BocStatusType = 'draft' | 'review' | 'success' | 'warning' | 'danger';

@Component({
  selector: 'boc-status-chip',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="boc-chip chip-{{ status }}"><ng-content></ng-content></span>`
})
export class BocStatusChipComponent {
  @Input() status: BocStatusType = 'draft';
}
