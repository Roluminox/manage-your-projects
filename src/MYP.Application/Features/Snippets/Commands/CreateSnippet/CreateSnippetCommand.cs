using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.Application.Features.Snippets.Commands.CreateSnippet;

public record CreateSnippetCommand(
    string Title,
    string Code,
    string Language,
    string? Description,
    List<Guid>? TagIds
) : IRequest<Result<SnippetDto>>;
