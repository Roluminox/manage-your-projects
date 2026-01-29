using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.Application.Features.Tags.Commands.CreateTag;

public record CreateTagCommand(
    string Name,
    string? Color
) : IRequest<Result<TagDto>>;
