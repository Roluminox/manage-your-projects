using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Snippets.Commands.DeleteSnippet;

public class DeleteSnippetCommandHandler : IRequestHandler<DeleteSnippetCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteSnippetCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteSnippetCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var snippet = await _context.Snippets
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == userId, cancellationToken);

        if (snippet is null)
        {
            return Result.Failure("Snippet not found.");
        }

        _context.Snippets.Remove(snippet);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
