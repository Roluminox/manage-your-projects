using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.AuditLogs.DTOs;

namespace MYP.Application.Features.AuditLogs.Queries.GetAuditLogsByEntity;

public record GetAuditLogsByEntityQuery(
    string EntityType,
    Guid EntityId
) : IRequest<Result<IReadOnlyList<AuditLogDto>>>;
