import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export enum Priority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export const PRIORITY_CONFIG: Record<Priority, { label: string; color: string; icon: string }> = {
  [Priority.Low]: {
    label: 'Low',
    color: '#22c55e',
    icon: 'M19 14l-7 7m0 0l-7-7m7 7V3'
  },
  [Priority.Medium]: {
    label: 'Medium',
    color: '#eab308',
    icon: 'M5 12h14'
  },
  [Priority.High]: {
    label: 'High',
    color: '#f97316',
    icon: 'M5 10l7-7m0 0l7 7m-7-7v18'
  },
  [Priority.Critical]: {
    label: 'Critical',
    color: '#ef4444',
    icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z'
  }
};

@Component({
  selector: 'app-priority-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="priority-badge"
      [class.small]="size === 'small'"
      [class.large]="size === 'large'"
      [class.icon-only]="iconOnly"
      [style.--priority-color]="config.color"
      [attr.title]="config.label"
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        stroke-width="2"
        stroke="currentColor"
        class="priority-icon"
      >
        <path stroke-linecap="round" stroke-linejoin="round" [attr.d]="config.icon" />
      </svg>
      @if (!iconOnly) {
        <span class="priority-text">{{ config.label }}</span>
      }
    </span>
  `,
  styles: [`
    .priority-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.375rem;
      padding: 0.25rem 0.5rem;
      font-size: 0.75rem;
      font-weight: 500;
      border-radius: 0.375rem;
      background: color-mix(in srgb, var(--priority-color) 15%, transparent);
      color: var(--priority-color);
      white-space: nowrap;
      transition: all 0.15s ease;
    }

    .priority-badge:hover {
      background: color-mix(in srgb, var(--priority-color) 25%, transparent);
    }

    .priority-badge.small {
      padding: 0.125rem 0.375rem;
      font-size: 0.6875rem;
      gap: 0.25rem;
    }

    .priority-badge.small .priority-icon {
      width: 0.75rem;
      height: 0.75rem;
    }

    .priority-badge.large {
      padding: 0.375rem 0.625rem;
      font-size: 0.8125rem;
      gap: 0.5rem;
    }

    .priority-badge.large .priority-icon {
      width: 1.125rem;
      height: 1.125rem;
    }

    .priority-badge.icon-only {
      padding: 0.375rem;
    }

    .priority-icon {
      width: 0.875rem;
      height: 0.875rem;
      flex-shrink: 0;
    }

    .priority-text {
      line-height: 1;
    }
  `]
})
export class PriorityBadgeComponent {
  @Input({ required: true }) priority!: Priority;
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() iconOnly = false;

  get config() {
    return PRIORITY_CONFIG[this.priority] || PRIORITY_CONFIG[Priority.Medium];
  }
}
