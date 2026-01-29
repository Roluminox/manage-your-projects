using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.Application.Features.Tasks.Queries.GetTaskById;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTaskByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<TaskItemDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<TaskItemDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var task = await _context.TaskItems
            .Include(t => t.Column)
                .ThenInclude(c => c.Project)
            .Include(t => t.Labels)
            .Include(t => t.Checklists.OrderBy(cl => cl.Order))
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.Column.Project.UserId == userId, cancellationToken);

        if (task is null)
        {
            return Result.Failure<TaskItemDto>("Task not found.");
        }

        var response = new TaskItemDto(
            Id: task.Id,
            Title: task.Title,
            Description: task.Description,
            Priority: task.Priority,
            DueDate: task.DueDate,
            Order: task.Order,
            IsArchived: task.IsArchived,
            CreatedAt: task.CreatedAt,
            UpdatedAt: task.UpdatedAt,
            Labels: task.Labels.Select(l => new LabelDto(l.Id, l.Name, l.Color)).ToList(),
            Checklists: task.Checklists
                .OrderBy(cl => cl.Order)
                .Select(cl => new ChecklistItemDto(cl.Id, cl.Text, cl.IsCompleted, cl.Order))
                .ToList()
        );

        return Result.Success(response);
    }
}
