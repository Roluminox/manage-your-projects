import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { AuditStateService } from './audit-state.service';
import { AuditApiService } from './audit-api.service';
import { AuditAction, AuditLogListResponse } from '../models/audit.models';

describe('AuditStateService', () => {
  let service: AuditStateService;
  let apiMock: {
    getAuditLogs: ReturnType<typeof vi.fn>;
    getAuditLogsByEntity: ReturnType<typeof vi.fn>;
  };

  const mockResponse: AuditLogListResponse = {
    items: [
      {
        id: '1',
        entityType: 'Snippet',
        entityId: 'entity-1',
        action: AuditAction.Created,
        oldValues: null,
        newValues: '{"Title":"Test"}',
        userId: 'user-1',
        userDisplayName: 'Test User',
        ipAddress: null,
        timestamp: '2026-02-08T10:00:00Z'
      }
    ],
    totalCount: 1,
    page: 1,
    pageSize: 20,
    totalPages: 1
  };

  beforeEach(() => {
    apiMock = {
      getAuditLogs: vi.fn(),
      getAuditLogsByEntity: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        AuditStateService,
        { provide: AuditApiService, useValue: apiMock }
      ]
    });

    service = TestBed.inject(AuditStateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have initial state', () => {
    expect(service.auditLogs()).toEqual([]);
    expect(service.loading()).toBe(false);
    expect(service.error()).toBeNull();
    expect(service.totalCount()).toBe(0);
  });

  it('should load audit logs successfully', async () => {
    apiMock.getAuditLogs.mockReturnValue(of(mockResponse));

    service.loadAuditLogs();

    await vi.waitFor(() => {
      expect(service.auditLogs()).toEqual(mockResponse.items);
      expect(service.totalCount()).toBe(1);
      expect(service.loading()).toBe(false);
    });
  });

  it('should handle load error', async () => {
    apiMock.getAuditLogs.mockReturnValue(throwError(() => ({ error: { errors: ['Load failed'] } })));

    service.loadAuditLogs();

    await vi.waitFor(() => {
      expect(service.error()).toBe('Load failed');
      expect(service.loading()).toBe(false);
    });
  });

  it('should update filters and reload', async () => {
    apiMock.getAuditLogs.mockReturnValue(of(mockResponse));

    service.setFilters({ entityType: 'Snippet' });

    await vi.waitFor(() => {
      expect(service.filters().entityType).toBe('Snippet');
      expect(apiMock.getAuditLogs).toHaveBeenCalled();
    });
  });

  it('should reset page when changing filters', async () => {
    apiMock.getAuditLogs.mockReturnValue(of(mockResponse));

    service.setFilters({ page: 3 });
    service.setFilters({ entityType: 'Snippet' });

    await vi.waitFor(() => {
      expect(service.currentPage()).toBe(1);
    });
  });

  it('should load entity history', async () => {
    const mockLogs = [mockResponse.items[0]];
    apiMock.getAuditLogsByEntity.mockReturnValue(of(mockLogs));

    service.loadEntityHistory('Snippet', 'entity-1');

    await vi.waitFor(() => {
      expect(service.entityHistory()).toEqual(mockLogs);
      expect(service.loading()).toBe(false);
    });
  });

  it('should clear error', () => {
    apiMock.getAuditLogs.mockReturnValue(throwError(() => ({ error: { errors: ['Error'] } })));
    service.loadAuditLogs();

    service.clearError();
    expect(service.error()).toBeNull();
  });
});
