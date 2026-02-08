import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditAction, AuditFilters, AUDIT_ACTION_LABELS, ENTITY_TYPES } from '../../models/audit.models';

@Component({
  selector: 'app-audit-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="filters">
      <select
        class="filter-select"
        [ngModel]="filters.entityType || ''"
        (ngModelChange)="onEntityTypeChange($event)"
      >
        <option value="">All Entities</option>
        @for (type of entityTypes; track type) {
          <option [value]="type">{{ type }}</option>
        }
      </select>

      <select
        class="filter-select"
        [ngModel]="selectedAction"
        (ngModelChange)="onActionChange($event)"
      >
        <option value="">All Actions</option>
        @for (action of actionOptions; track action.value) {
          <option [value]="action.value">{{ action.label }}</option>
        }
      </select>

      <input
        type="date"
        class="filter-input"
        [ngModel]="filters.fromDate || ''"
        (ngModelChange)="onFromDateChange($event)"
        placeholder="From date"
      />

      <input
        type="date"
        class="filter-input"
        [ngModel]="filters.toDate || ''"
        (ngModelChange)="onToDateChange($event)"
        placeholder="To date"
      />

      <button class="btn-reset" (click)="resetFilters.emit()">Reset</button>
    </div>
  `,
  styles: [`
    .filters {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
      margin-bottom: 1.5rem;
    }
    .filter-select, .filter-input {
      padding: 0.5rem 0.75rem;
      border: 1px solid var(--color-border, #e2e8f0);
      border-radius: 0.5rem;
      font-size: 0.875rem;
      background: var(--color-bg-primary, white);
      color: var(--color-text-primary, #1a202c);
    }
    .filter-select:focus, .filter-input:focus {
      outline: none;
      border-color: var(--color-primary, #6366f1);
      box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.1);
    }
    .btn-reset {
      padding: 0.5rem 1rem;
      border: 1px solid var(--color-border, #e2e8f0);
      border-radius: 0.5rem;
      background: transparent;
      color: var(--color-text-secondary, #64748b);
      cursor: pointer;
      font-size: 0.875rem;
    }
    .btn-reset:hover {
      background: var(--color-bg-secondary, #f8fafc);
    }
  `]
})
export class AuditFiltersComponent {
  @Input() filters: AuditFilters = { page: 1, pageSize: 20 };
  @Output() filtersChange = new EventEmitter<Partial<AuditFilters>>();
  @Output() resetFilters = new EventEmitter<void>();

  entityTypes = ENTITY_TYPES;
  actionOptions = [
    { value: AuditAction.Created, label: AUDIT_ACTION_LABELS[AuditAction.Created] },
    { value: AuditAction.Updated, label: AUDIT_ACTION_LABELS[AuditAction.Updated] },
    { value: AuditAction.Deleted, label: AUDIT_ACTION_LABELS[AuditAction.Deleted] },
  ];

  get selectedAction(): string {
    return this.filters.action !== undefined ? this.filters.action.toString() : '';
  }

  onEntityTypeChange(value: string): void {
    this.filtersChange.emit({ entityType: value || undefined });
  }

  onActionChange(value: string): void {
    this.filtersChange.emit({ action: value ? Number(value) : undefined });
  }

  onFromDateChange(value: string): void {
    this.filtersChange.emit({ fromDate: value || undefined });
  }

  onToDateChange(value: string): void {
    this.filtersChange.emit({ toDate: value || undefined });
  }
}
