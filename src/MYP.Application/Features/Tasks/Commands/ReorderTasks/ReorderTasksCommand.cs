using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Tasks.Commands.ReorderTasks;

public record ReorderTasksCommand(
    Guid ColumnId,
    List<Guid> TaskIds
) : IRequest<Result>;
