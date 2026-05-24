import { Component } from '@angular/core';
import { CommonModule, AsyncPipe } from '@angular/common';
import { ToastService, Toast } from '../../services/toast.service';

@Component({
  selector: 'app-toast-system',
  standalone: true,
  imports: [CommonModule, AsyncPipe],
  templateUrl: './toast-system.component.html'
})
export class ToastSystemComponent {
  toasts$ = this.toastService.toasts$;

  constructor(private toastService: ToastService) {}

  dismiss(id: string): void {
    this.toastService.dismiss(id);
  }

  trackById(index: number, toast: Toast): string {
    return toast.id;
  }
}
