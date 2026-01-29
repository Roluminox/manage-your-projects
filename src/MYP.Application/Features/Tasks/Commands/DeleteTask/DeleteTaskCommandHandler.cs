using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteTaskCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var task = await _context.TaskItems
            .Include(t => t.Column)
                .ThenInclude(c => c.Project)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.Column.Project.UserId == userId, cancellationToken);

        if (task is null)
        {
            return Result.Failure("Task not found.");
        }

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
