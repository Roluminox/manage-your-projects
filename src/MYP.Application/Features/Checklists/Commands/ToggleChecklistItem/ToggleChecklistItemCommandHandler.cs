using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Checklists.Commands.ToggleChecklistItem;

public class ToggleChecklistItemCommandHandler : IRequestHandler<ToggleChecklistItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ToggleChecklistItemCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ToggleChecklistItemCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var checklistItem = await _context.ChecklistItems
            .Include(c => c.TaskItem)
                .ThenInclude(t => t.Column)
                    .ThenInclude(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.TaskItem.Column.Project.UserId == userId, cancellationToken);

        if (checklistItem is null)
        {
            return Result.Failure("Checklist item not found.");
        }

        checklistItem.Toggle();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
