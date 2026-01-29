using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Snippets.Commands.UpdateSnippet;

public class UpdateSnippetCommandHandler : IRequestHandler<UpdateSnippetCommand, Result<SnippetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateSnippetCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SnippetDto>> Handle(UpdateSnippetCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<SnippetDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        // Find the snippet
        var snippet = await _context.Snippets
            .Include(s => s.Tags)
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == userId, cancellationToken);

        if (snippet is null)
        {
            return Result.Failure<SnippetDto>("Snippet not found.");
        }

        // Update snippet properties
        snippet.Update(
            request.Title,
            request.Code,
            request.Language.ToLowerInvariant(),
            request.Description
        );

        // Update tags if provided
        List<Tag> tags = new();
        if (request.TagIds is not null)
        {
            tags = await _context.Tags
                .Where(t => t.UserId == userId && request.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            snippet.SetTags(tags);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Build response
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
