import { Injectable, signal } from '@angular/core';

export type ToastKind = 'default' | 'success' | 'danger' | 'warning';

export interface Toast {
  id: number;
  text: string;
  kind: ToastKind;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toasts = signal<Toast[]>([]);
  private nextId = 1;

  show(text: string, kind: ToastKind = 'default', delay = 5000): void {
    const toast: Toast = { id: this.nextId++, text, kind };
    this.toasts.update(list => [...list, toast]);
    setTimeout(() => this.remove(toast.id), delay);
  }

  remove(id: number): void {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }
}
