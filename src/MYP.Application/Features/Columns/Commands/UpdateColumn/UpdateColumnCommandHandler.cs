using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.Application.Features.Columns.Commands.UpdateColumn;

public class UpdateColumnCommandHandler : IRequestHandler<UpdateColumnCommand, Result<ColumnDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateColumnCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ColumnDto>> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<ColumnDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var column = await _context.Columns
            .Include(c => c.Project)
            .Include(c => c.Tasks.Where(t => !t.IsArchived).OrderBy(t => t.Order))
                .ThenInclude(t => t.Labels)
            .Include(c => c.Tasks)
                .ThenInclude(t => t.Checklists.OrderBy(cl => cl.Order))
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.Project.UserId == userId, cancellationToken);

        if (column is null)
        {
            return Result.Failure<ColumnDto>("Column not found.");
        }

        column.Name = request.Name.Trim();
        column.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new ColumnDto(
            Id: column.Id,
            Name: column.Name,
            Order: column.Order,
            Tasks: column.Tasks
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
        );

        return Result.Success(response);
    }
}
