using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MYP.Application.Features.Projects.DTOs;
using MYP.Application.Features.Tasks.Commands.ArchiveTask;
using MYP.Application.Features.Tasks.Commands.CreateTask;
using MYP.Application.Features.Tasks.Commands.DeleteTask;
using MYP.Application.Features.Tasks.Commands.MoveTask;
using MYP.Application.Features.Tasks.Commands.ReorderTasks;
using MYP.Application.Features.Tasks.Commands.UpdateTask;
using MYP.Application.Features.Tasks.Queries.GetTaskById;
using MYP.Domain.Enums;

namespace MYP.API.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("tasks/{id:guid}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTaskByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpPost("columns/{columnId:guid}/tasks")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTask(Guid columnId, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand(
            ColumnId: columnId,
            Title: request.Title,
            Description: request.Description,
            Priority: request.Priority,
            DueDate: request.DueDate,
            LabelIds: request.LabelIds
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Contains("Column not found.")
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return Created($"/api/tasks/{result.Value.Id}", result.Value);
    }

    [HttpPut("tasks/{id:guid}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(
            Id: id,
            Title: request.Title,
            Description: request.Description,
            Priority: request.Priority,
            DueDate: request.DueDate,
            LabelIds: request.LabelIds
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Contains("Task not found.")
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpDelete("tasks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteTaskCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return NoContent();
    }

    [HttpPut("tasks/{id:guid}/move")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveTask(Guid id, [FromBody] MoveTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new MoveTaskCommand(
            TaskId: id,
            TargetColumnId: request.TargetColumnId,
            NewOrder: request.NewOrder
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Any(e => e.Contains("not found"))
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }

    [HttpPut("columns/{columnId:guid}/tasks/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderTasks(Guid columnId, [FromBody] ReorderTasksRequest request, CancellationToken cancellationToken)
    {
        var command = new ReorderTasksCommand(
            ColumnId: columnId,
            TaskIds: request.TaskIds
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Contains("Column not found.")
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }

    [HttpPut("tasks/{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveTask(Guid id, [FromBody] ArchiveTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new ArchiveTaskCommand(id, request.Archive);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return NoContent();
    }
}

public record CreateTaskRequest(
    string Title,
    string? Description,
    Priority Priority,
    DateTime? DueDate,
    List<Guid>? LabelIds
);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    Priority Priority,
    DateTime? DueDate,
    List<Guid>? LabelIds
);

public record MoveTaskRequest(Guid TargetColumnId, int NewOrder);
public record ReorderTasksRequest(List<Guid> TaskIds);
public record ArchiveTaskRequest(bool Archive);
