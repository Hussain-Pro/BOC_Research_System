import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'boc-auth-shell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './boc-auth-shell.component.html',
  styleUrl: './boc-auth-shell.component.scss'
})
export class BocAuthShellComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() visualTitle = 'نظام إدارة البحوث';
  @Input() visualSubtitle = 'شركة نفط البصرة — منصة التقييم البحثي';
  @Input() maxWidth = '420px';
  @Input() showVisual = true;
}
