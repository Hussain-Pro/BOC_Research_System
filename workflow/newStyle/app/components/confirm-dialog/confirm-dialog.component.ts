import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss']
})
export class ConfirmDialogComponent {
  @Input() isOpen = false;
  @Input() title = 'CONFIRM.TITLE';
  @Input() message = 'CONFIRM.MESSAGE';
  @Input() confirmText = 'CONFIRM.CONFIRM';
  @Input() cancelText = 'CONFIRM.CANCEL';
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  closeOnBackdrop(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.cancel();
  }

  confirm(): void {
    this.confirmed.emit();
    this.isOpen = false;
  }

  cancel(): void {
    this.cancelled.emit();
    this.isOpen = false;
  }
}
