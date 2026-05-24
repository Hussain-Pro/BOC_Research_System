import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

export interface PageAction {
  label: string;
  variant: 'primary' | 'secondary' | 'tertiary' | 'danger';
  icon?: string;
  size?: 'sm' | 'md';
  disabled?: boolean;
  handler: () => void;
}

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './page-header.component.html',
  styleUrls: ['./page-header.component.scss']
})
export class PageHeaderComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() actions: PageAction[] = [];
}
