using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.Application.Features.Snippets.Queries.SearchSnippets;

public record SearchSnippetsQuery(
    string SearchTerm,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<SnippetListResponse>>;
