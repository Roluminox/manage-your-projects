import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { signal, computed } from '@angular/core';
import { AuditListComponent } from './audit-list.component';
import { AuditStateService } from '../../services/audit-state.service';

describe('AuditListComponent', () => {
  let component: AuditListComponent;
  let fixture: ComponentFixture<AuditListComponent>;
  let stateMock: Partial<AuditStateService>;

  beforeEach(async () => {
    const filtersSignal = signal({ page: 1, pageSize: 20 });

    stateMock = {
      auditLogs: computed(() => []),
      totalCount: computed(() => 0),
      totalPages: computed(() => 0),
      filters: computed(() => filtersSignal()),
      loading: computed(() => false),
      error: computed(() => null),
      currentPage: computed(() => 1),
      hasNextPage: computed(() => false),
      hasPreviousPage: computed(() => false),
      loadAuditLogs: vi.fn(),
      setFilters: vi.fn(),
      resetFilters: vi.fn(),
      clearError: vi.fn(),
      nextPage: vi.fn(),
      previousPage: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [AuditListComponent],
      providers: [
        { provide: AuditStateService, useValue: stateMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AuditListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load audit logs on init', () => {
    expect(stateMock.loadAuditLogs).toHaveBeenCalled();
  });

  it('should display empty state when no logs', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.empty-state')).toBeTruthy();
    expect(compiled.textContent).toContain('No audit logs found');
  });
});
