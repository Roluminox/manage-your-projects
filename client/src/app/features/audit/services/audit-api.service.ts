import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuditFilters, AuditLog, AuditLogListResponse } from '../models/audit.models';

@Injectable({
  providedIn: 'root'
})
export class AuditApiService {
  private readonly http = inject(HttpClient);
  private readonly auditUrl = `${environment.apiUrl}/audit-logs`;

  getAuditLogs(filters: Partial<AuditFilters> = {}): Observable<AuditLogListResponse> {
    let params = new HttpParams();

    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    if (filters.entityType) params = params.set('entityType', filters.entityType);
    if (filters.action !== undefined) params = params.set('action', filters.action.toString());
    if (filters.fromDate) params = params.set('fromDate', filters.fromDate);
    if (filters.toDate) params = params.set('toDate', filters.toDate);

    return this.http.get<AuditLogListResponse>(this.auditUrl, { params });
  }

  getAuditLogsByEntity(entityType: string, entityId: string): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.auditUrl}/${entityType}/${entityId}`);
  }
}
