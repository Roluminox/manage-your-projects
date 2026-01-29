using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Tasks.Commands.MoveTask;

public class MoveTaskCommandHandler : IRequestHandler<MoveTaskCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MoveTaskCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        // Load task with source column
        var task = await _context.TaskItems
            .Include(t => t.Column)
                .ThenInclude(c => c.Project)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.Column.Project.UserId == userId, cancellationToken);

        if (task is null)
        {
            return Result.Failure("Task not found.");
        }

        // Load target column
        var targetColumn = await _context.Columns
            .Include(c => c.Project)
            .Include(c => c.Tasks.Where(t => !t.IsArchived))
            .FirstOrDefaultAsync(c => c.Id == request.TargetColumnId && c.Project.UserId == userId, cancellationToken);

        if (targetColumn is null)
        {
            return Result.Failure("Target column not found.");
        }

        // Verify both columns belong to the same project
        if (task.Column.ProjectId != targetColumn.ProjectId)
        {
            return Result.Failure("Cannot move task to a column in a different project.");
        }

        var sourceColumnId = task.ColumnId;
        var isMovingToNewColumn = sourceColumnId != request.TargetColumnId;

        if (isMovingToNewColumn)
        {
            // Reorder tasks in source column (close the gap)
            var sourceTasks = await _context.TaskItems
                .Where(t => t.ColumnId == sourceColumnId && !t.IsArchived && t.Order > task.Order)
                .ToListAsync(cancellationToken);

            foreach (var t in sourceTasks)
            {
                t.Order--;
                t.UpdatedAt = DateTime.UtcNow;
            }

            // Update task's column
            task.ColumnId = request.TargetColumnId;
        }

        // Make room in target column
        var targetTasks = targetColumn.Tasks
            .Where(t => !t.IsArchived && t.Id != task.Id && t.Order >= request.NewOrder)
            .ToList();

        foreach (var t in targetTasks)
        {
            t.Order++;
            t.UpdatedAt = DateTime.UtcNow;
        }

        // Set new order
        task.Order = request.NewOrder;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
