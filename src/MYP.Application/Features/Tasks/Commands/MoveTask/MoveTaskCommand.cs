using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Tasks.Commands.MoveTask;

public record MoveTaskCommand(
    Guid TaskId,
    Guid TargetColumnId,
    int NewOrder
) : IRequest<Result>;
