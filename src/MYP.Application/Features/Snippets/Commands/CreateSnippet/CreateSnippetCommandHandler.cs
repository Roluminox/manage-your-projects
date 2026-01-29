using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Snippets.Commands.CreateSnippet;

public class CreateSnippetCommandHandler : IRequestHandler<CreateSnippetCommand, Result<SnippetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateSnippetCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SnippetDto>> Handle(CreateSnippetCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<SnippetDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        // Load tags if provided
        var tags = new List<Tag>();
        if (request.TagIds?.Any() == true)
        {
            tags = await _context.Tags
                .Where(t => t.UserId == userId && request.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);
        }

        // Create snippet
        var snippet = new Snippet
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Code = request.Code,
            Language = request.Language.ToLowerInvariant(),
            Description = request.Description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        // Add tags
        foreach (var tag in tags)
        {
            snippet.Tags.Add(tag);
        }

        _context.Snippets.Add(snippet);
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
            Tags: tags.Select(t => new TagDto(t.Id, t.Name, t.Color)).ToList()
        );

        return Result.Success(response);
    }
}
