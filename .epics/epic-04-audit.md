# Epic 04: Audit Logs

## Objectif
Implementer le systeme d'audit logs automatique qui capture toutes les mutations (Create, Update, Delete) avec les anciennes et nouvelles valeurs.

## Prerequis
- [x] Epic 00 complete
- [x] Epic 01 complete
- [x] Epic 02 complete (ou en parallele)
- [x] Epic 03 complete (ou en parallele)

## Criteres de Validation
- [x] Chaque mutation genere un log
- [x] Diff visible entre anciennes et nouvelles valeurs
- [x] Pagination et filtres fonctionnels

---

## Taches

### Backend - Domain & Application

- [x] **TASK-04-001**: Creer l'entite AuditLog
  - Fichiers: `src/MYP.Domain/Entities/AuditLog.cs`
  - Tests: `tests/MYP.Domain.Tests/Entities/AuditLogTests.cs`
  - Specs:
    - Id: GUID
    - EntityType: string (ex: "Snippet", "TaskItem")
    - EntityId: GUID
    - Action: enum (Created, Updated, Deleted)
    - OldValues: JSON?
    - NewValues: JSON?
    - UserId: GUID
    - IpAddress: string?
    - UserAgent: string?
    - Timestamp: DateTime

- [x] **TASK-04-002**: Creer l'enum AuditAction
  - Fichiers: `src/MYP.Domain/Enums/AuditAction.cs`
  - Valeurs: Created, Updated, Deleted

- [x] **TASK-04-003**: Creer l'interface IAuditableEntity
  - Fichiers: `src/MYP.Domain/Interfaces/IAuditableEntity.cs`
  - Proprietes: CreatedAt, UpdatedAt

- [x] **TASK-04-004**: Creer GetAuditLogsQuery + Handler
  - Fichiers: `src/MYP.Application/Features/AuditLogs/Queries/GetAuditLogs/`
  - Tests: `tests/MYP.Application.Tests/Features/AuditLogs/Queries/GetAuditLogs/`
  - Specs: Pagination, filtres par EntityType, Action, date range

- [x] **TASK-04-005**: Creer GetAuditLogsByEntityQuery + Handler
  - Fichiers: `src/MYP.Application/Features/AuditLogs/Queries/GetAuditLogsByEntity/`
  - Tests: `tests/MYP.Application.Tests/Features/AuditLogs/Queries/GetAuditLogsByEntity/`
  - Specs: Historique d'une entite specifique

- [x] **TASK-04-006**: Creer les DTOs AuditLog
  - Fichiers: `src/MYP.Application/Features/AuditLogs/DTOs/`
  - Tests: Mapping tests

### Backend - Infrastructure & API

- [x] **TASK-04-007**: Creer AuditLogConfiguration
  - Fichiers: `src/MYP.Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs`
  - Tests: `tests/MYP.Infrastructure.Tests/`
  - Specs: Index sur UserId, (EntityType, EntityId), Timestamp

- [x] **TASK-04-008**: Creer la migration AuditLogs
  - Fichiers: `src/MYP.Infrastructure/Persistence/Migrations/`
  - Commande: `dotnet ef migrations add AddAuditLogs`

- [x] **TASK-04-009**: Implementer AuditableEntityInterceptor
  - Fichiers: `src/MYP.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs`
  - Tests: `tests/MYP.Infrastructure.Tests/Persistence/Interceptors/`
  - Specs:
    - Intercepte SaveChanges
    - Capture Added, Modified, Deleted entities
    - Serialise OldValues/NewValues en JSON
    - Recupere UserId via ICurrentUserService

- [x] **TASK-04-010**: Configurer l'interceptor dans ApplicationDbContext
  - Fichiers: `src/MYP.Infrastructure/Persistence/ApplicationDbContext.cs`
  - Tests: Integration tests

- [x] **TASK-04-011**: Creer AuditLogsController
  - Fichiers: `src/MYP.API/Controllers/AuditLogsController.cs`
  - Tests: `tests/MYP.API.Tests/Controllers/AuditLogsControllerTests.cs`
  - Endpoints:
    - GET `/api/audit-logs` (paginated, filtered)
    - GET `/api/audit-logs/{entityType}/{entityId}`

### Frontend

- [x] **TASK-04-100**: Creer AuditApiService
  - Fichiers: `client/src/app/features/audit/services/audit-api.service.ts`
  - Tests: `client/src/app/features/audit/services/audit-api.service.spec.ts`

- [x] **TASK-04-101**: Creer AuditStateService (Signals)
  - Fichiers: `client/src/app/features/audit/services/audit-state.service.ts`
  - Tests: `client/src/app/features/audit/services/audit-state.service.spec.ts`

- [x] **TASK-04-102**: Creer AuditListPage
  - Fichiers: `client/src/app/features/audit/pages/audit-list/`
  - Tests: `client/src/app/features/audit/pages/audit-list/audit-list.component.spec.ts`

- [x] **TASK-04-103**: Creer AuditLogEntry component
  - Fichiers: `client/src/app/features/audit/components/audit-log-entry/`
  - Tests: `client/src/app/features/audit/components/audit-log-entry/audit-log-entry.component.spec.ts`

- [x] **TASK-04-104**: Implementer l'affichage JSON diff
  - Fichiers: `client/src/app/features/audit/components/json-diff/`
  - Tests: `client/src/app/features/audit/components/json-diff/json-diff.component.spec.ts`
  - Specs: Afficher old vs new values avec highlighting des differences

- [x] **TASK-04-105**: Implementer les filtres
  - Fichiers: `client/src/app/features/audit/components/audit-filters/`
  - Tests: `client/src/app/features/audit/components/audit-filters/audit-filters.component.spec.ts`
  - Specs: Filtre par type d'entite, action, date range

- [x] **TASK-04-106**: Configurer les routes Audit
  - Fichiers: `client/src/app/features/audit/audit.routes.ts`

### Tests

- [x] **TASK-04-200**: Tests - Logs crees automatiquement sur Create
  - Fichiers: `tests/MYP.Infrastructure.Tests/Persistence/Interceptors/AuditableEntityInterceptorTests.cs`
  - Cas: Creation d'un Snippet -> AuditLog avec Action=Created, NewValues

- [x] **TASK-04-201**: Tests - Logs crees automatiquement sur Update
  - Fichiers: `tests/MYP.Infrastructure.Tests/Persistence/Interceptors/`
  - Cas: Update d'un Snippet -> AuditLog avec Action=Updated, OldValues, NewValues

- [x] **TASK-04-202**: Tests - Logs crees automatiquement sur Delete
  - Fichiers: `tests/MYP.Infrastructure.Tests/Persistence/Interceptors/`
  - Cas: Delete d'un Snippet -> AuditLog avec Action=Deleted, OldValues

- [x] **TASK-04-203**: Tests - OldValues/NewValues sont corrects
  - Fichiers: `tests/MYP.Infrastructure.Tests/Persistence/Interceptors/`
  - Cas: Verifier que le JSON contient les bonnes proprietes

- [x] **TASK-04-204**: Tests d'integration - GetAuditLogs avec pagination
  - Fichiers: `tests/MYP.API.Tests/Integration/AuditLogs/`

- [x] **TASK-04-205**: Tests d'integration - GetAuditLogs avec filtres
  - Fichiers: `tests/MYP.API.Tests/Integration/AuditLogs/`

- [x] **TASK-04-206**: Tests d'integration - GetAuditLogsByEntity
  - Fichiers: `tests/MYP.API.Tests/Integration/AuditLogs/`

- [x] **TASK-04-207**: Tests Frontend - AuditListPage
  - Fichiers: `client/src/app/features/audit/pages/audit-list/audit-list.component.spec.ts`

- [x] **TASK-04-208**: Tests Frontend - JSON diff component
  - Fichiers: `client/src/app/features/audit/components/json-diff/json-diff.component.spec.ts`

---

## Notes

- L'interceptor EF Core est la cle de cette feature
- Le JSON doit etre stocke en JSONB pour PostgreSQL (meilleure performance)
- Penser a la retention des logs (optionnel: suppression auto apres 90 jours)
- Ne pas logger les proprietes sensibles (PasswordHash, etc.)
