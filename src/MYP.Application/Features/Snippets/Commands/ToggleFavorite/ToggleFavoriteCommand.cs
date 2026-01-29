using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Snippets.Commands.ToggleFavorite;

public record ToggleFavoriteCommand(Guid Id) : IRequest<Result<bool>>;
