using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Columns.Commands.DeleteColumn;

public class DeleteColumnCommandHandler : IRequestHandler<DeleteColumnCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteColumnCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var column = await _context.Columns
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.Project.UserId == userId, cancellationToken);

        if (column is null)
        {
            return Result.Failure("Column not found.");
        }

        _context.Columns.Remove(column);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
