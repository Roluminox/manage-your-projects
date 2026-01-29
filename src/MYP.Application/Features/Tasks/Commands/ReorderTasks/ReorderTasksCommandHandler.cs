using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Tasks.Commands.ReorderTasks;

public class ReorderTasksCommandHandler : IRequestHandler<ReorderTasksCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReorderTasksCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ReorderTasksCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var column = await _context.Columns
            .Include(c => c.Project)
            .Include(c => c.Tasks.Where(t => !t.IsArchived))
            .FirstOrDefaultAsync(c => c.Id == request.ColumnId && c.Project.UserId == userId, cancellationToken);

        if (column is null)
        {
            return Result.Failure("Column not found.");
        }

        var columnTaskIds = column.Tasks.Select(t => t.Id).ToHashSet();
        var requestTaskIds = request.TaskIds.ToHashSet();

        // Verify all provided task IDs belong to the column
        if (!requestTaskIds.IsSubsetOf(columnTaskIds))
        {
            return Result.Failure("Some task IDs do not belong to this column.");
        }

        // Verify all non-archived column tasks are included
        if (!columnTaskIds.IsSubsetOf(requestTaskIds))
        {
            return Result.Failure("All column tasks must be included in the reorder.");
        }

        // Update order
        for (int i = 0; i < request.TaskIds.Count; i++)
        {
            var task = column.Tasks.First(t => t.Id == request.TaskIds[i]);
            task.Order = i;
            task.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
