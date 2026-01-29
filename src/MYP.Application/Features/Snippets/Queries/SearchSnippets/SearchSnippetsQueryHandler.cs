using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.Application.Features.Snippets.Queries.SearchSnippets;

public class SearchSnippetsQueryHandler : IRequestHandler<SearchSnippetsQuery, Result<SnippetListResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SearchSnippetsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SnippetListResponse>> Handle(SearchSnippetsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<SnippetListResponse>("User is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return Result.Failure<SnippetListResponse>("Search term is required.");
        }

        var userId = _currentUser.UserId.Value;
        var searchTerm = request.SearchTerm.ToLowerInvariant();

        // Search in title, code, and description (case-insensitive)
        var query = _context.Snippets
            .Include(s => s.Tags)
            .Where(s => s.UserId == userId)
            .Where(s =>
                s.Title.ToLower().Contains(searchTerm) ||
                s.Code.ToLower().Contains(searchTerm) ||
                (s.Description != null && s.Description.ToLower().Contains(searchTerm)));

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var snippets = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = snippets.Select(s => new SnippetSummaryDto(
            Id: s.Id,
            Title: s.Title,
            Language: s.Language,
            IsFavorite: s.IsFavorite,
            CreatedAt: s.CreatedAt,
            Tags: s.Tags.Select(t => new TagDto(t.Id, t.Name, t.Color)).ToList()
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new SnippetListResponse(
            Items: items,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        );

        return Result.Success(response);
    }
}
