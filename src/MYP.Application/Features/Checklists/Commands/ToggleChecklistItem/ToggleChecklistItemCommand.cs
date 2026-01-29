using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Checklists.Commands.ToggleChecklistItem;

public record ToggleChecklistItemCommand(Guid Id) : IRequest<Result>;
