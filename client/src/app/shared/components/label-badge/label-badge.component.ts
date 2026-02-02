import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface LabelBadgeData {
  name: string;
  color: string;
}

@Component({
  selector: 'app-label-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="label-badge"
      [class.small]="size === 'small'"
      [class.large]="size === 'large'"
      [style.--label-color]="label.color"
      [attr.title]="label.name"
    >
      <span class="label-dot"></span>
      @if (!compact) {
        <span class="label-text">{{ label.name }}</span>
      }
    </span>
  `,
  styles: [`
    .label-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.375rem;
      padding: 0.25rem 0.5rem;
      font-size: 0.75rem;
      font-weight: 500;
      border-radius: 9999px;
      background: color-mix(in srgb, var(--label-color) 15%, transparent);
      color: color-mix(in srgb, var(--label-color) 80%, var(--text-primary, #1e293b));
      white-space: nowrap;
      transition: all 0.15s ease;
    }

    .label-badge:hover {
      background: color-mix(in srgb, var(--label-color) 25%, transparent);
    }

    .label-badge.small {
      padding: 0.125rem 0.375rem;
      font-size: 0.6875rem;
      gap: 0.25rem;
    }

    .label-badge.small .label-dot {
      width: 0.375rem;
      height: 0.375rem;
    }

    .label-badge.large {
      padding: 0.375rem 0.75rem;
      font-size: 0.8125rem;
      gap: 0.5rem;
    }

    .label-badge.large .label-dot {
      width: 0.625rem;
      height: 0.625rem;
    }

    .label-dot {
      width: 0.5rem;
      height: 0.5rem;
      border-radius: 50%;
      background: var(--label-color);
      flex-shrink: 0;
    }

    .label-text {
      max-width: 120px;
      overflow: hidden;
      text-overflow: ellipsis;
    }
  `]
})
export class LabelBadgeComponent {
  @Input({ required: true }) label!: LabelBadgeData;
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() compact = false;
}
