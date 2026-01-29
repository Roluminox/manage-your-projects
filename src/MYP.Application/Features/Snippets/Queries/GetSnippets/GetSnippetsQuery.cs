using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.Application.Features.Snippets.Queries.GetSnippets;

public record GetSnippetsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Language = null,
    Guid? TagId = null,
    bool? IsFavorite = null,
    string? SortBy = null,
    bool SortDescending = true
) : IRequest<Result<SnippetListResponse>>;
