import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChecklistItem } from '../../models/kanban.models';

@Component({
  selector: 'app-checklist-item',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="checklist-item" [class.completed]="item.isCompleted">
      <input
        type="checkbox"
        class="checkbox"
        [checked]="item.isCompleted"
        (change)="onToggle()"
        [attr.aria-label]="'Toggle ' + item.text"
      />
      <span class="item-text">{{ item.text }}</span>
      <button
        type="button"
        class="btn-delete"
        (click)="onDelete()"
        title="Delete item"
        aria-label="Delete checklist item"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>
  `,
  styles: [`
    .checklist-item {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 0.625rem;
      background: var(--bg-secondary, #f8fafc);
      border-radius: 0.375rem;
      transition: background-color 0.15s ease;
    }

    .checklist-item:hover {
      background: var(--bg-tertiary, #f1f5f9);
    }

    .checkbox {
      width: 1.125rem;
      height: 1.125rem;
      cursor: pointer;
      accent-color: var(--primary, #6366f1);
      flex-shrink: 0;
    }

    .item-text {
      flex: 1;
      font-size: 0.875rem;
      color: var(--text-primary, #1e293b);
      line-height: 1.4;
      word-break: break-word;
    }

    .checklist-item.completed .item-text {
      text-decoration: line-through;
      color: var(--text-muted, #94a3b8);
    }

    .btn-delete {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 0.25rem;
      background: transparent;
      border: none;
      border-radius: 0.25rem;
      color: var(--text-muted, #94a3b8);
      cursor: pointer;
      opacity: 0;
      transition: all 0.15s ease;
      flex-shrink: 0;
    }

    .checklist-item:hover .btn-delete {
      opacity: 1;
    }

    .btn-delete:hover {
      background: #fef2f2;
      color: var(--danger, #ef4444);
    }

    .btn-delete:focus {
      opacity: 1;
      outline: 2px solid var(--primary, #6366f1);
      outline-offset: 2px;
    }

    .btn-delete svg {
      width: 0.875rem;
      height: 0.875rem;
    }
  `]
})
export class ChecklistItemComponent {
  @Input({ required: true }) item!: ChecklistItem;

  @Output() toggle = new EventEmitter<void>();
  @Output() delete = new EventEmitter<void>();

  onToggle(): void {
    this.toggle.emit();
  }

  onDelete(): void {
    this.delete.emit();
  }
}
