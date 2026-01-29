using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTaskCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<TaskItemDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<TaskItemDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var column = await _context.Columns
            .Include(c => c.Project)
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == request.ColumnId && c.Project.UserId == userId, cancellationToken);

        if (column is null)
        {
            return Result.Failure<TaskItemDto>("Column not found.");
        }

        // Load labels if provided
        var labels = new List<Label>();
        if (request.LabelIds?.Any() == true)
        {
            labels = await _context.Labels
                .Where(l => l.ProjectId == column.ProjectId && request.LabelIds.Contains(l.Id))
                .ToListAsync(cancellationToken);
        }

        var maxOrder = column.Tasks.Where(t => !t.IsArchived).Any()
            ? column.Tasks.Where(t => !t.IsArchived).Max(t => t.Order)
            : -1;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            DueDate = request.DueDate,
            Order = maxOrder + 1,
            IsArchived = false,
            ColumnId = column.Id,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var label in labels)
        {
            task.Labels.Add(label);
        }

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

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
            Labels: labels.Select(l => new LabelDto(l.Id, l.Name, l.Color)).ToList(),
            Checklists: new List<ChecklistItemDto>()
        );

        return Result.Success(response);
    }
}
