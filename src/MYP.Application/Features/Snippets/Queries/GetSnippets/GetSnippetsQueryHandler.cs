using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Snippets.Queries.GetSnippets;

public class GetSnippetsQueryHandler : IRequestHandler<GetSnippetsQuery, Result<SnippetListResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSnippetsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SnippetListResponse>> Handle(GetSnippetsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<SnippetListResponse>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var query = _context.Snippets
            .Include(s => s.Tags)
            .Where(s => s.UserId == userId)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Language))
        {
            query = query.Where(s => s.Language == request.Language.ToLowerInvariant());
        }

        if (request.TagId.HasValue)
        {
            query = query.Where(s => s.Tags.Any(t => t.Id == request.TagId.Value));
        }

        if (request.IsFavorite.HasValue)
        {
            query = query.Where(s => s.IsFavorite == request.IsFavorite.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var snippets = await query
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

    private static IQueryable<Snippet> ApplySorting(IQueryable<Snippet> query, string? sortBy, bool sortDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "title" => sortDescending
                ? query.OrderByDescending(s => s.Title)
                : query.OrderBy(s => s.Title),
            "language" => sortDescending
                ? query.OrderByDescending(s => s.Language)
                : query.OrderBy(s => s.Language),
            "updatedat" => sortDescending
                ? query.OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
                : query.OrderBy(s => s.UpdatedAt ?? s.CreatedAt),
            _ => sortDescending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt)
        };
    }
}
