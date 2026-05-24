import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

export type StatusVariant =
  | 'draft' | 'review' | 'success' | 'danger' | 'warning'
  | 'triage' | 'evaluator' | 'committee' | 'ministry' | 'export' | 'plagiarism';

@Component({
  selector: 'app-status-pill',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './status-pill.component.html',
  styleUrls: ['./status-pill.component.scss']
})
export class StatusPillComponent {
  @Input() variant: StatusVariant = 'draft';
  @Input() label = '';
}
