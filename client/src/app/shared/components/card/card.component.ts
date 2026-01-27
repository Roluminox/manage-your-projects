import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="cardClasses">
      @if (hasHeader) {
        <div class="px-4 py-3 border-b border-[var(--color-border)]">
          <ng-content select="[card-header]"></ng-content>
        </div>
      }
      <div [class]="bodyClasses">
        <ng-content></ng-content>
      </div>
      @if (hasFooter) {
        <div class="px-4 py-3 border-t border-[var(--color-border)] bg-[var(--color-bg-secondary)]">
          <ng-content select="[card-footer]"></ng-content>
        </div>
      }
    </div>
  `
})
export class CardComponent {
  @Input() hoverable = false;
  @Input() padding: 'none' | 'sm' | 'md' | 'lg' = 'md';
  @Input() hasHeader = false;
  @Input() hasFooter = false;

  get cardClasses(): string {
    const base = 'bg-[var(--color-bg-primary)] border border-[var(--color-border)] rounded-lg shadow-[var(--shadow-sm)] transition-all duration-150';
    const hover = this.hoverable ? 'hover:shadow-[var(--shadow-md)] hover:border-[var(--color-border-hover)] cursor-pointer' : '';

    return `${base} ${hover}`;
  }

  get bodyClasses(): string {
    const paddings: Record<string, string> = {
      none: '',
      sm: 'p-2',
      md: 'p-4',
      lg: 'p-6'
    };

    return paddings[this.padding];
  }
}
