using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.AuditLogs.DTOs;
using MYP.Domain.Enums;

namespace MYP.Application.Features.AuditLogs.Queries.GetAuditLogs;

public record GetAuditLogsQuery(
    int Page = 1,
    int PageSize = 20,
    string? EntityType = null,
    AuditAction? Action = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<Result<AuditLogListResponse>>;
