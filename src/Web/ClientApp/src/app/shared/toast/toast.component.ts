import { Component } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  standalone: false,
  selector: 'app-toasts',
  template: `
    <div class="toast-container" aria-live="polite">
      @for (toast of toasts.toasts(); track toast.id) {
        <article class="toast toast-{{ toast.kind }}">
          <span>{{ toast.text }}</span>
          <button type="button" class="icon-btn" aria-label="Cerrar" (click)="toasts.remove(toast.id)">×</button>
        </article>
      }
    </div>
  `
})
export class ToastComponent {
  constructor(public toasts: ToastService) {}
}
