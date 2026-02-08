using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.AuditLogs.DTOs;

namespace MYP.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<AuditLogListResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAuditLogsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<AuditLogListResponse>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<AuditLogListResponse>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var query = _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }

        if (request.Action.HasValue)
        {
            query = query.Where(a => a.Action == request.Action.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.ToDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var auditLogs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = auditLogs.Select(a => new AuditLogDto(
            Id: a.Id,
            EntityType: a.EntityType,
            EntityId: a.EntityId,
            Action: a.Action,
            OldValues: a.OldValues,
            NewValues: a.NewValues,
            UserId: a.UserId,
            UserDisplayName: a.User?.DisplayName,
            IpAddress: a.IpAddress,
            Timestamp: a.Timestamp
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new AuditLogListResponse(
            Items: items,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        );

        return Result.Success(response);
    }
}
