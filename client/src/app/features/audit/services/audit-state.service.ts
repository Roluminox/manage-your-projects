import { Injectable, computed, inject, signal } from '@angular/core';
import { catchError, finalize, of, tap } from 'rxjs';
import { AuditFilters, AuditLog } from '../models/audit.models';
import { AuditApiService } from './audit-api.service';

interface AuditState {
  auditLogs: AuditLog[];
  entityHistory: AuditLog[];
  totalCount: number;
  totalPages: number;
  filters: AuditFilters;
  loading: boolean;
  error: string | null;
}

const initialFilters: AuditFilters = {
  page: 1,
  pageSize: 20,
};

const initialState: AuditState = {
  auditLogs: [],
  entityHistory: [],
  totalCount: 0,
  totalPages: 0,
  filters: initialFilters,
  loading: false,
  error: null,
};

@Injectable({
  providedIn: 'root'
})
export class AuditStateService {
  private readonly api = inject(AuditApiService);

  private readonly state = signal<AuditState>(initialState);

  readonly auditLogs = computed(() => this.state().auditLogs);
  readonly entityHistory = computed(() => this.state().entityHistory);
  readonly totalCount = computed(() => this.state().totalCount);
  readonly totalPages = computed(() => this.state().totalPages);
  readonly filters = computed(() => this.state().filters);
  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);

  readonly currentPage = computed(() => this.state().filters.page);
  readonly hasNextPage = computed(() => this.state().filters.page < this.state().totalPages);
  readonly hasPreviousPage = computed(() => this.state().filters.page > 1);

  loadAuditLogs(): void {
    this.updateState({ loading: true, error: null });

    this.api.getAuditLogs(this.state().filters).pipe(
      tap(response => {
        this.updateState({
          auditLogs: response.items,
          totalCount: response.totalCount,
          totalPages: response.totalPages,
        });
      }),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to load audit logs' });
        return of(null);
      }),
      finalize(() => this.updateState({ loading: false }))
    ).subscribe();
  }

  loadEntityHistory(entityType: string, entityId: string): void {
    this.updateState({ loading: true, error: null, entityHistory: [] });

    this.api.getAuditLogsByEntity(entityType, entityId).pipe(
      tap(logs => this.updateState({ entityHistory: logs })),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to load entity history' });
        return of(null);
      }),
      finalize(() => this.updateState({ loading: false }))
    ).subscribe();
  }

  setFilters(filters: Partial<AuditFilters>): void {
    const currentFilters = this.state().filters;
    const newFilters = { ...currentFilters, ...filters };

    if (filters.entityType !== undefined || filters.action !== undefined ||
        filters.fromDate !== undefined || filters.toDate !== undefined) {
      newFilters.page = 1;
    }

    this.updateState({ filters: newFilters });
    this.loadAuditLogs();
  }

  setPage(page: number): void {
    this.setFilters({ page });
  }

  nextPage(): void {
    if (this.hasNextPage()) {
      this.setPage(this.currentPage() + 1);
    }
  }

  previousPage(): void {
    if (this.hasPreviousPage()) {
      this.setPage(this.currentPage() - 1);
    }
  }

  resetFilters(): void {
    this.updateState({ filters: initialFilters });
    this.loadAuditLogs();
  }

  clearError(): void {
    this.updateState({ error: null });
  }

  private updateState(partialState: Partial<AuditState>): void {
    this.state.update(state => ({ ...state, ...partialState }));
  }
}
