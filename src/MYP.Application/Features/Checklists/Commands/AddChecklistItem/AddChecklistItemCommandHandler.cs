using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Checklists.Commands.AddChecklistItem;

public class AddChecklistItemCommandHandler : IRequestHandler<AddChecklistItemCommand, Result<ChecklistItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddChecklistItemCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ChecklistItemDto>> Handle(AddChecklistItemCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<ChecklistItemDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var task = await _context.TaskItems
            .Include(t => t.Column)
                .ThenInclude(c => c.Project)
            .Include(t => t.Checklists)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.Column.Project.UserId == userId, cancellationToken);

        if (task is null)
        {
            return Result.Failure<ChecklistItemDto>("Task not found.");
        }

        var maxOrder = task.Checklists.Any()
            ? task.Checklists.Max(c => c.Order)
            : -1;

        var checklistItem = new ChecklistItem
        {
            Id = Guid.NewGuid(),
            Text = request.Text.Trim(),
            IsCompleted = false,
            Order = maxOrder + 1,
            TaskItemId = task.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChecklistItems.Add(checklistItem);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new ChecklistItemDto(
            Id: checklistItem.Id,
            Text: checklistItem.Text,
            IsCompleted: checklistItem.IsCompleted,
            Order: checklistItem.Order
        );

        return Result.Success(response);
    }
}
