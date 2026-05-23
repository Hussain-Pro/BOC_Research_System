import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'boc-stat-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './boc-stat-card.component.html',
  styleUrl: './boc-stat-card.component.scss'
})
export class BocStatCardComponent {
  @Input() label = '';
  @Input() value: string | number = '';
  @Input() icon = 'bi-graph-up';
  @Input() iconBg = 'rgba(15, 42, 56, 0.1)';
  @Input() iconColor = 'var(--boc-primary)';
  @Input() trend = '';
  @Input() trendUp = true;
}
