using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Columns.Commands.ReorderColumns;

public class ReorderColumnsCommandHandler : IRequestHandler<ReorderColumnsCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReorderColumnsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ReorderColumnsCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var project = await _context.Projects
            .Include(p => p.Columns)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.UserId == userId, cancellationToken);

        if (project is null)
        {
            return Result.Failure("Project not found.");
        }

        var projectColumnIds = project.Columns.Select(c => c.Id).ToHashSet();
        var requestColumnIds = request.ColumnIds.ToHashSet();

        // Verify all provided column IDs belong to the project
        if (!requestColumnIds.IsSubsetOf(projectColumnIds))
        {
            return Result.Failure("Some column IDs do not belong to this project.");
        }

        // Verify all project columns are included
        if (!projectColumnIds.IsSubsetOf(requestColumnIds))
        {
            return Result.Failure("All project columns must be included in the reorder.");
        }

        // Update order
        for (int i = 0; i < request.ColumnIds.Count; i++)
        {
            var column = project.Columns.First(c => c.Id == request.ColumnIds[i]);
            column.Order = i;
            column.UpdatedAt = DateTime.UtcNow;
        }

        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
