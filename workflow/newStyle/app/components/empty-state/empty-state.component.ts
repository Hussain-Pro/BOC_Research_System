import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './empty-state.component.html',
  styleUrls: ['./empty-state.component.scss']
})
export class EmptyStateComponent {
  @Input() icon = 'bi bi-inbox';
  @Input() title = 'EMPTY.TITLE';
  @Input() subtitle = 'EMPTY.SUBTITLE';
  @Input() actionLabel = '';
  @Input() actionIcon = '';
  @Input() actionVariant: 'tertiary' | 'primary' = 'tertiary';
  @Output() actionClick = new EventEmitter<void>();
}
