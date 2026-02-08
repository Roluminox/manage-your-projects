using MYP.Domain.Enums;

namespace MYP.Application.Features.AuditLogs.DTOs;

public record AuditLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    AuditAction Action,
    string? OldValues,
    string? NewValues,
    Guid UserId,
    string? UserDisplayName,
    string? IpAddress,
    DateTime Timestamp
);

public record AuditLogListResponse(
    IReadOnlyList<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
