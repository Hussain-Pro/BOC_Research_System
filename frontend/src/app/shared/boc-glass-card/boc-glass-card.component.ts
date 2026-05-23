import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'boc-glass-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="glass-card" [class.p-0]="noPadding" [ngClass]="extraClass">
      <div *ngIf="title" class="card-header-custom px-4 pt-4 pb-2">
        <h5 class="fw-bold mb-0">{{ title }}</h5>
        <p class="text-muted small mb-0 mt-1" *ngIf="subtitle">{{ subtitle }}</p>
      </div>
      <div [class.p-4]="!noPadding" [class.pt-2]="title && !noPadding">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .card-header-custom h5 { color: var(--boc-primary); }
  `]
})
export class BocGlassCardComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() noPadding = false;
  @Input() extraClass = '';
}
