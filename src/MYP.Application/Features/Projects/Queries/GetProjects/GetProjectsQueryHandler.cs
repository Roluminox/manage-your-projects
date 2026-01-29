using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, Result<List<ProjectSummaryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProjectsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ProjectSummaryDto>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<List<ProjectSummaryDto>>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var projects = await _context.Projects
            .Where(p => p.UserId == userId)
            .Include(p => p.Columns)
                .ThenInclude(c => c.Tasks.Where(t => !t.IsArchived))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectSummaryDto(
                p.Id,
                p.Name,
                p.Description,
                p.Color,
                p.CreatedAt,
                p.Columns.Count,
                p.Columns.SelectMany(c => c.Tasks).Count(t => !t.IsArchived)
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(projects);
    }
}
