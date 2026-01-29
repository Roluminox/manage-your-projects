using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.Application.Features.Snippets.Queries.GetSnippetById;

public record GetSnippetByIdQuery(Guid Id) : IRequest<Result<SnippetDto>>;
