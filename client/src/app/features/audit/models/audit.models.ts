export enum AuditAction {
  Created = 0,
  Updated = 1,
  Deleted = 2
}

export interface AuditLog {
  id: string;
  entityType: string;
  entityId: string;
  action: AuditAction;
  oldValues: string | null;
  newValues: string | null;
  userId: string;
  userDisplayName: string | null;
  ipAddress: string | null;
  timestamp: string;
}

export interface AuditLogListResponse {
  items: AuditLog[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AuditFilters {
  page: number;
  pageSize: number;
  entityType?: string;
  action?: AuditAction;
  fromDate?: string;
  toDate?: string;
}

export const AUDIT_ACTION_LABELS: Record<AuditAction, string> = {
  [AuditAction.Created]: 'Created',
  [AuditAction.Updated]: 'Updated',
  [AuditAction.Deleted]: 'Deleted',
};

export const ENTITY_TYPES = [
  'Snippet',
  'Tag',
  'Project',
  'Column',
  'TaskItem',
  'Label',
  'ChecklistItem',
] as const;
