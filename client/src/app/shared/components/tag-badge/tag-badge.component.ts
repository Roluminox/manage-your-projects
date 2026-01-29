import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-tag-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="tag-badge"
      [style.background-color]="color()"
      [style.color]="textColor()"
      [class.removable]="removable()"
    >
      {{ name() }}
      @if (removable()) {
        <button
          type="button"
          class="remove-btn"
          (click)="onRemove($event)"
          aria-label="Remove tag"
        >
          &times;
        </button>
      }
    </span>
  `,
  styles: [`
    .tag-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      padding: 0.25rem 0.5rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 500;
      line-height: 1;
    }

    .tag-badge.removable {
      padding-right: 0.25rem;
    }

    .remove-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 1rem;
      height: 1rem;
      padding: 0;
      border: none;
      background: rgba(0, 0, 0, 0.2);
      color: inherit;
      border-radius: 50%;
      cursor: pointer;
      font-size: 0.875rem;
      line-height: 1;
      transition: background-color 0.15s ease;
    }

    .remove-btn:hover {
      background: rgba(0, 0, 0, 0.3);
    }
  `]
})
export class TagBadgeComponent {
  readonly name = input.required<string>();
  readonly color = input<string>('#6366f1');
  readonly removable = input<boolean>(false);

  readonly remove = output<void>();

  textColor(): string {
    const hex = this.color().replace('#', '');
    const r = parseInt(hex.substring(0, 2), 16);
    const g = parseInt(hex.substring(2, 4), 16);
    const b = parseInt(hex.substring(4, 6), 16);
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
    return luminance > 0.5 ? '#000000' : '#ffffff';
  }

  onRemove(event: Event): void {
    event.stopPropagation();
    this.remove.emit();
  }
}
