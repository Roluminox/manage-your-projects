using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.Application.Features.Snippets.Commands.UpdateSnippet;

public record UpdateSnippetCommand(
    Guid Id,
    string Title,
    string Code,
    string Language,
    string? Description,
    List<Guid>? TagIds
) : IRequest<Result<SnippetDto>>;
