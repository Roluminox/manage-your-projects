using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.AuditLogs.DTOs;

namespace MYP.Application.Features.AuditLogs.Queries.GetAuditLogsByEntity;

public class GetAuditLogsByEntityQueryHandler : IRequestHandler<GetAuditLogsByEntityQuery, Result<IReadOnlyList<AuditLogDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAuditLogsByEntityQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> Handle(GetAuditLogsByEntityQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<IReadOnlyList<AuditLogDto>>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var auditLogs = await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.UserId == userId
                && a.EntityType == request.EntityType
                && a.EntityId == request.EntityId)
            .OrderByDescending(a => a.Timestamp)
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

        return Result.Success<IReadOnlyList<AuditLogDto>>(items);
    }
}
