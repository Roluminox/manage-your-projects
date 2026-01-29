using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.Application.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProjectByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<ProjectDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var project = await _context.Projects
            .Where(p => p.Id == request.Id && p.UserId == userId)
            .Include(p => p.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Tasks.Where(t => !t.IsArchived).OrderBy(t => t.Order))
                    .ThenInclude(t => t.Labels)
            .Include(p => p.Columns)
                .ThenInclude(c => c.Tasks)
                    .ThenInclude(t => t.Checklists.OrderBy(cl => cl.Order))
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            return Result.Failure<ProjectDto>("Project not found.");
        }

        var response = new ProjectDto(
            Id: project.Id,
            Name: project.Name,
            Description: project.Description,
            Color: project.Color,
            CreatedAt: project.CreatedAt,
            UpdatedAt: project.UpdatedAt,
            Columns: project.Columns
                .OrderBy(c => c.Order)
                .Select(c => new ColumnDto(
                    Id: c.Id,
                    Name: c.Name,
                    Order: c.Order,
                    Tasks: c.Tasks
                        .Where(t => !t.IsArchived)
                        .OrderBy(t => t.Order)
                        .Select(t => new TaskItemDto(
                            Id: t.Id,
                            Title: t.Title,
                            Description: t.Description,
                            Priority: t.Priority,
                            DueDate: t.DueDate,
                            Order: t.Order,
                            IsArchived: t.IsArchived,
                            CreatedAt: t.CreatedAt,
                            UpdatedAt: t.UpdatedAt,
                            Labels: t.Labels.Select(l => new LabelDto(l.Id, l.Name, l.Color)).ToList(),
                            Checklists: t.Checklists
                                .OrderBy(cl => cl.Order)
                                .Select(cl => new ChecklistItemDto(cl.Id, cl.Text, cl.IsCompleted, cl.Order))
                                .ToList()
                        )).ToList()
                )).ToList()
        );

        return Result.Success(response);
    }
}
