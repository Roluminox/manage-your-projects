import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastType } from './toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-4 right-4 z-[100] flex flex-col gap-2 max-w-sm">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          [class]="getToastClasses(toast.type)"
          class="animate-slide-in"
        >
          <div class="flex items-start gap-3">
            <!-- Icon -->
            <div [class]="getIconClasses(toast.type)">
              @switch (toast.type) {
                @case ('success') {
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                  </svg>
                }
                @case ('error') {
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                  </svg>
                }
                @case ('warning') {
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
                  </svg>
                }
                @case ('info') {
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                  </svg>
                }
              }
            </div>

            <!-- Message -->
            <p class="flex-1 text-sm font-medium">{{ toast.message }}</p>

            <!-- Close button -->
            <button
              type="button"
              class="text-current opacity-70 hover:opacity-100 transition-opacity"
              (click)="toastService.dismiss(toast.id)"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </button>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    @keyframes slide-in {
      from {
        opacity: 0;
        transform: translateX(100%);
      }
      to {
        opacity: 1;
        transform: translateX(0);
      }
    }

    .animate-slide-in {
      animation: slide-in 0.2s ease-out;
    }
  `]
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);

  getToastClasses(type: ToastType): string {
    const base = 'px-4 py-3 rounded-lg shadow-lg border';

    const types: Record<ToastType, string> = {
      success: 'bg-green-50 border-green-200 text-green-800 dark:bg-green-900/30 dark:border-green-800 dark:text-green-200',
      error: 'bg-red-50 border-red-200 text-red-800 dark:bg-red-900/30 dark:border-red-800 dark:text-red-200',
      warning: 'bg-yellow-50 border-yellow-200 text-yellow-800 dark:bg-yellow-900/30 dark:border-yellow-800 dark:text-yellow-200',
      info: 'bg-blue-50 border-blue-200 text-blue-800 dark:bg-blue-900/30 dark:border-blue-800 dark:text-blue-200'
    };

    return `${base} ${types[type]}`;
  }

  getIconClasses(type: ToastType): string {
    const types: Record<ToastType, string> = {
      success: 'text-green-500',
      error: 'text-red-500',
      warning: 'text-yellow-500',
      info: 'text-blue-500'
    };

    return types[type];
  }
}
