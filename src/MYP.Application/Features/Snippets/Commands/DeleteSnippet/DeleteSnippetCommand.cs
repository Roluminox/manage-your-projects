using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Snippets.Commands.DeleteSnippet;

public record DeleteSnippetCommand(Guid Id) : IRequest<Result>;
