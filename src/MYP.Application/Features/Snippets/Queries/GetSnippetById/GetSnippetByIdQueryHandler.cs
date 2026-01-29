using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.Application.Features.Snippets.Queries.GetSnippetById;

public class GetSnippetByIdQueryHandler : IRequestHandler<GetSnippetByIdQuery, Result<SnippetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSnippetByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SnippetDto>> Handle(GetSnippetByIdQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<SnippetDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var snippet = await _context.Snippets
            .Include(s => s.Tags)
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == userId, cancellationToken);

        if (snippet is null)
        {
            return Result.Failure<SnippetDto>("Snippet not found.");
        }

        var response = new SnippetDto(
            Id: snippet.Id,
            Title: snippet.Title,
            Code: snippet.Code,
            Language: snippet.Language,
            Description: snippet.Description,
            IsFavorite: snippet.IsFavorite,
            CreatedAt: snippet.CreatedAt,
            UpdatedAt: snippet.UpdatedAt,
            Tags: snippet.Tags.Select(t => new TagDto(t.Id, t.Name, t.Color)).ToList()
        );

        return Result.Success(response);
    }
}
