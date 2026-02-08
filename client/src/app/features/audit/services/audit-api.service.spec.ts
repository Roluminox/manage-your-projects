import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuditApiService } from './audit-api.service';
import { AuditAction } from '../models/audit.models';
import { environment } from '../../../../environments/environment';

describe('AuditApiService', () => {
  let service: AuditApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuditApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(AuditApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch audit logs without filters', () => {
    service.getAuditLogs().subscribe(response => {
      expect(response.items.length).toBe(1);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/audit-logs`);
    expect(req.request.method).toBe('GET');
    req.flush({ items: [{ id: '1' }], totalCount: 1, page: 1, pageSize: 20, totalPages: 1 });
  });

  it('should fetch audit logs with filters', () => {
    service.getAuditLogs({ entityType: 'Snippet', action: AuditAction.Created, page: 2 }).subscribe();

    const req = httpMock.expectOne(r =>
      r.url === `${environment.apiUrl}/audit-logs` &&
      r.params.get('entityType') === 'Snippet' &&
      r.params.get('action') === '0' &&
      r.params.get('page') === '2'
    );
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], totalCount: 0, page: 2, pageSize: 20, totalPages: 0 });
  });

  it('should fetch audit logs by entity', () => {
    const entityId = '123-456';
    service.getAuditLogsByEntity('Snippet', entityId).subscribe(logs => {
      expect(logs.length).toBe(2);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/audit-logs/Snippet/${entityId}`);
    expect(req.request.method).toBe('GET');
    req.flush([{ id: '1' }, { id: '2' }]);
  });
});
