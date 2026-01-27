import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
  duration: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private nextId = 0;

  readonly toasts = signal<Toast[]>([]);

  show(message: string, type: ToastType = 'info', duration = 5000): number {
    const id = this.nextId++;

    const toast: Toast = { id, type, message, duration };

    this.toasts.update(toasts => [...toasts, toast]);

    if (duration > 0) {
      setTimeout(() => this.dismiss(id), duration);
    }

    return id;
  }

  success(message: string, duration = 5000): number {
    return this.show(message, 'success', duration);
  }

  error(message: string, duration = 7000): number {
    return this.show(message, 'error', duration);
  }

  warning(message: string, duration = 5000): number {
    return this.show(message, 'warning', duration);
  }

  info(message: string, duration = 5000): number {
    return this.show(message, 'info', duration);
  }

  dismiss(id: number): void {
    this.toasts.update(toasts => toasts.filter(t => t.id !== id));
  }

  dismissAll(): void {
    this.toasts.set([]);
  }
}
