import { Component, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditAction, AuditLog, AUDIT_ACTION_LABELS } from '../../models/audit.models';
import { JsonDiffComponent } from '../json-diff/json-diff.component';

@Component({
  selector: 'app-audit-log-entry',
  standalone: true,
  imports: [CommonModule, JsonDiffComponent],
  template: `
    <div class="entry" (click)="expanded.set(!expanded())">
      <div class="entry-header">
        <span class="action-badge" [class]="'action-' + actionClass">
          {{ actionLabel }}
        </span>
        <span class="entity-type">{{ log.entityType }}</span>
        <span class="entity-id" title="{{ log.entityId }}">{{ log.entityId | slice:0:8 }}...</span>
        <span class="spacer"></span>
        @if (log.userDisplayName) {
          <span class="user">{{ log.userDisplayName }}</span>
        }
        <span class="timestamp">{{ log.timestamp | date:'medium' }}</span>
        <span class="expand-icon" [class.rotated]="expanded()">&#9660;</span>
      </div>

      @if (expanded()) {
        <div class="entry-details" (click)="$event.stopPropagation()">
          <app-json-diff
            [oldValues]="log.oldValues"
            [newValues]="log.newValues"
          />
          @if (log.ipAddress) {
            <div class="meta">IP: {{ log.ipAddress }}</div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .entry {
      border: 1px solid var(--color-border, #e2e8f0);
      border-radius: 0.5rem;
      background: var(--color-bg-primary, white);
      cursor: pointer;
      transition: box-shadow 0.15s;
    }
    .entry:hover {
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
    }
    .entry-header {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
    }
    .action-badge {
      padding: 0.2rem 0.6rem;
      border-radius: 0.375rem;
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: uppercase;
    }
    .action-created { background: #dcfce7; color: #166534; }
    .action-updated { background: #fef9c3; color: #854d0e; }
    .action-deleted { background: #fee2e2; color: #991b1b; }
    .entity-type {
      font-weight: 600;
      color: var(--color-text-primary, #334155);
    }
    .entity-id {
      font-family: monospace;
      font-size: 0.8rem;
      color: var(--color-text-secondary, #94a3b8);
    }
    .spacer { flex: 1; }
    .user {
      font-size: 0.85rem;
      color: var(--color-text-secondary, #64748b);
    }
    .timestamp {
      font-size: 0.8rem;
      color: var(--color-text-secondary, #94a3b8);
    }
    .expand-icon {
      font-size: 0.6rem;
      transition: transform 0.2s;
      color: var(--color-text-secondary, #94a3b8);
    }
    .expand-icon.rotated { transform: rotate(180deg); }
    .entry-details {
      padding: 0.75rem 1rem;
      border-top: 1px solid var(--color-border, #e2e8f0);
      background: var(--color-bg-secondary, #f8fafc);
    }
    .meta {
      margin-top: 0.5rem;
      font-size: 0.75rem;
      color: var(--color-text-secondary, #94a3b8);
    }
  `]
})
export class AuditLogEntryComponent {
  @Input({ required: true }) log!: AuditLog;

  expanded = signal(false);

  get actionLabel(): string {
    return AUDIT_ACTION_LABELS[this.log.action] || 'Unknown';
  }

  get actionClass(): string {
    switch (this.log.action) {
      case AuditAction.Created: return 'created';
      case AuditAction.Updated: return 'updated';
      case AuditAction.Deleted: return 'deleted';
      default: return '';
    }
  }
}
