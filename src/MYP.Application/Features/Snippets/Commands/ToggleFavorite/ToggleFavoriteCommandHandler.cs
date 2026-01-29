using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Snippets.Commands.ToggleFavorite;

public class ToggleFavoriteCommandHandler : IRequestHandler<ToggleFavoriteCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ToggleFavoriteCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<bool>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var snippet = await _context.Snippets
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == userId, cancellationToken);

        if (snippet is null)
        {
            return Result.Failure<bool>("Snippet not found.");
        }

        snippet.ToggleFavorite();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(snippet.IsFavorite);
    }
}
