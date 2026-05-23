import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface BocBreadcrumb {
  label: string;
  route?: string;
}

@Component({
  selector: 'boc-page-hero',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './boc-page-hero.component.html',
  styleUrl: './boc-page-hero.component.scss'
})
export class BocPageHeroComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() breadcrumbs: BocBreadcrumb[] = [];
}
