import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  id: string;
  type: 'info' | 'success' | 'warning' | 'error';
  title: string;
  message: string;
  duration: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastsSubject = new BehaviorSubject<Toast[]>([]);
  toasts$ = this.toastsSubject.asObservable();

  show(message: string, type: Toast['type'] = 'info', title?: string, duration?: number): void {
    const titles: Record<string, string> = {
      info: 'TOAST.INFO',
      success: 'TOAST.SUCCESS',
      warning: 'TOAST.WARNING',
      error: 'TOAST.ERROR'
    };

    const defaultDurations: Record<string, number> = {
      info: 5000,
      success: 5000,
      warning: 10000,
      error: 0 // manual dismiss
    };

    const toast: Toast = {
      id: this.generateId(),
      type,
      title: title || titles[type],
      message,
      duration: duration ?? defaultDurations[type]
    };

    const current = this.toastsSubject.value;
    this.toastsSubject.next([...current, toast]);

    if (toast.duration > 0) {
      setTimeout(() => this.remove(toast.id), toast.duration);
    }
  }

  remove(id: string): void {
    const current = this.toastsSubject.value;
    this.toastsSubject.next(current.filter(t => t.id !== id));
  }

  private generateId(): string {
    return 'toast-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
  }
}
