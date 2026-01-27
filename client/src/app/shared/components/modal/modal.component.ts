import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (isOpen) {
      <div class="fixed inset-0 z-50 overflow-y-auto">
        <!-- Backdrop -->
        <div
          class="fixed inset-0 bg-black/50 transition-opacity"
          [class.animate-fade-in]="isOpen"
          (click)="onBackdropClick()"
        ></div>

        <!-- Modal container -->
        <div class="flex min-h-full items-center justify-center p-4">
          <div
            [class]="modalClasses"
            [class.animate-slide-up]="isOpen"
            (click)="$event.stopPropagation()"
          >
            <!-- Header -->
            @if (title) {
              <div class="flex items-center justify-between px-6 py-4 border-b border-[var(--color-border)]">
                <h3 class="text-lg font-semibold text-[var(--color-text-primary)]">{{ title }}</h3>
                @if (showCloseButton) {
                  <button
                    type="button"
                    class="text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)] transition-colors"
                    (click)="close()"
                  >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                  </button>
                }
              </div>
            }

            <!-- Body -->
            <div class="px-6 py-4">
              <ng-content></ng-content>
            </div>

            <!-- Footer -->
            @if (hasFooter) {
              <div class="px-6 py-4 border-t border-[var(--color-border)] bg-[var(--color-bg-secondary)] rounded-b-lg">
                <ng-content select="[modal-footer]"></ng-content>
              </div>
            }
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    @keyframes fade-in {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes slide-up {
      from {
        opacity: 0;
        transform: translateY(10px) scale(0.95);
      }
      to {
        opacity: 1;
        transform: translateY(0) scale(1);
      }
    }

    .animate-fade-in {
      animation: fade-in 0.15s ease-out;
    }

    .animate-slide-up {
      animation: slide-up 0.2s ease-out;
    }
  `]
})
export class ModalComponent {
  @Input() isOpen = false;
  @Input() title = '';
  @Input() size: 'sm' | 'md' | 'lg' | 'xl' = 'md';
  @Input() showCloseButton = true;
  @Input() closeOnBackdrop = true;
  @Input() hasFooter = false;

  @Output() closed = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.isOpen) {
      this.close();
    }
  }

  get modalClasses(): string {
    const base = 'relative bg-[var(--color-bg-primary)] rounded-lg shadow-[var(--shadow-lg)] w-full';

    const sizes: Record<string, string> = {
      sm: 'max-w-sm',
      md: 'max-w-md',
      lg: 'max-w-lg',
      xl: 'max-w-xl'
    };

    return `${base} ${sizes[this.size]}`;
  }

  onBackdropClick(): void {
    if (this.closeOnBackdrop) {
      this.close();
    }
  }

  close(): void {
    this.closed.emit();
  }
}
