import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'boc-empty-state',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './boc-empty-state.component.html'
})
export class BocEmptyStateComponent {
  @Input() icon = 'bi-inbox';
  @Input() title = 'لا توجد بيانات';
  @Input() message = 'لم يتم العثور على أي عناصر لعرضها.';
  @Input() actionLabel = '';
  @Input() actionRoute = '';
}
