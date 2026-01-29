using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Checklists.Commands.DeleteChecklistItem;

public record DeleteChecklistItemCommand(Guid Id) : IRequest<Result>;
