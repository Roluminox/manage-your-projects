using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Tasks.Commands.ArchiveTask;

public record ArchiveTaskCommand(
    Guid TaskId,
    bool Archive
) : IRequest<Result>;
