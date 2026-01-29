using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;
using MYP.Domain.Enums;

namespace MYP.Application.Features.Tasks.Commands.UpdateTask;

public record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    Priority Priority,
    DateTime? DueDate,
    List<Guid>? LabelIds
) : IRequest<Result<TaskItemDto>>;
