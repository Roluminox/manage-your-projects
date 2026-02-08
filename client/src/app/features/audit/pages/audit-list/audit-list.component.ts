import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditStateService } from '../../services/audit-state.service';
import { AuditFiltersComponent } from '../../components/audit-filters/audit-filters.component';
import { AuditLogEntryComponent } from '../../components/audit-log-entry/audit-log-entry.component';
import { AuditFilters } from '../../models/audit.models';

@Component({
  selector: 'app-audit-list',
  standalone: true,
  imports: [CommonModule, AuditFiltersComponent, AuditLogEntryComponent],
  template: `
    <div class="audit-page">
      <header class="page-header">
        <h1 class="page-title">Audit Logs</h1>
        <span class="total-count">{{ state.totalCount() }} entries</span>
      </header>

      @if (state.error()) {
        <div class="error-banner">
          {{ state.error() }}
          <button class="close-btn" (click)="state.clearError()">x</button>
        </div>
      }

      <app-audit-filters
        [filters]="state.filters()"
        (filtersChange)="onFiltersChange($event)"
        (resetFilters)="state.resetFilters()"
      />

      @if (state.loading()) {
        <div class="loading-state">
          <div class="spinner"></div>
          <span>Loading audit logs...</span>
        </div>
      } @else if (state.auditLogs().length === 0) {
        <div class="empty-state">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="empty-icon">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25z" />
          </svg>
          <h2>No audit logs found</h2>
          <p>Audit logs will appear here when you create, update, or delete entities.</p>
        </div>
      } @else {
        <div class="logs-list">
          @for (log of state.auditLogs(); track log.id) {
            <app-audit-log-entry [log]="log" />
          }
        </div>

        @if (state.totalPages() > 1) {
          <div class="pagination">
            <button
              class="btn btn-secondary"
              [disabled]="!state.hasPreviousPage()"
              (click)="state.previousPage()"
            >
              Previous
            </button>
            <span class="page-info">
              Page {{ state.currentPage() }} of {{ state.totalPages() }}
            </span>
            <button
              class="btn btn-secondary"
              [disabled]="!state.hasNextPage()"
              (click)="state.nextPage()"
            >
              Next
            </button>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .audit-page {
      padding: 1.5rem;
      max-width: 1200px;
      margin: 0 auto;
    }
    .page-header {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 1.5rem;
    }
    .page-title {
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--color-text-primary, #1e293b);
    }
    .total-count {
      font-size: 0.875rem;
      color: var(--color-text-secondary, #94a3b8);
      background: var(--color-bg-secondary, #f1f5f9);
      padding: 0.25rem 0.75rem;
      border-radius: 1rem;
    }
    .error-banner {
      display: flex;
      align-items: center;
      justify-content: space-between;
      background: #fee2e2;
      color: #991b1b;
      padding: 0.75rem 1rem;
      border-radius: 0.5rem;
      margin-bottom: 1rem;
    }
    .close-btn {
      background: none;
      border: none;
      cursor: pointer;
      color: inherit;
      font-size: 1rem;
    }
    .loading-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1rem;
      padding: 3rem;
      color: var(--color-text-secondary, #64748b);
    }
    .spinner {
      width: 2rem;
      height: 2rem;
      border: 3px solid var(--color-border, #e2e8f0);
      border-top: 3px solid var(--color-primary, #6366f1);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .empty-state {
      text-align: center;
      padding: 4rem 2rem;
      color: var(--color-text-secondary, #64748b);
    }
    .empty-icon {
      width: 3rem;
      height: 3rem;
      margin-bottom: 1rem;
      color: var(--color-text-secondary, #94a3b8);
    }
    .empty-state h2 {
      font-size: 1.125rem;
      font-weight: 600;
      color: var(--color-text-primary, #334155);
      margin-bottom: 0.5rem;
    }
    .logs-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .pagination {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      margin-top: 1.5rem;
      padding: 1rem;
    }
    .btn {
      padding: 0.5rem 1rem;
      border-radius: 0.5rem;
      font-size: 0.875rem;
      cursor: pointer;
      border: 1px solid var(--color-border, #e2e8f0);
    }
    .btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    .btn-secondary {
      background: var(--color-bg-primary, white);
      color: var(--color-text-primary, #334155);
    }
    .btn-secondary:hover:not(:disabled) {
      background: var(--color-bg-secondary, #f8fafc);
    }
    .page-info {
      font-size: 0.875rem;
      color: var(--color-text-secondary, #64748b);
    }
  `]
})
export class AuditListComponent implements OnInit {
  readonly state = inject(AuditStateService);

  ngOnInit(): void {
    this.state.loadAuditLogs();
  }

  onFiltersChange(filters: Partial<AuditFilters>): void {
    this.state.setFilters(filters);
  }
}
