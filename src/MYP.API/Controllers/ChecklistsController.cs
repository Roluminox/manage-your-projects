using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MYP.Application.Features.Checklists.Commands.AddChecklistItem;
using MYP.Application.Features.Checklists.Commands.DeleteChecklistItem;
using MYP.Application.Features.Checklists.Commands.ToggleChecklistItem;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.API.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class ChecklistsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChecklistsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("tasks/{taskId:guid}/checklist")]
    [ProducesResponseType(typeof(ChecklistItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddChecklistItem(Guid taskId, [FromBody] AddChecklistItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddChecklistItemCommand(
            TaskId: taskId,
            Text: request.Text
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Contains("Task not found.")
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return Created($"/api/checklist/{result.Value.Id}", result.Value);
    }

    [HttpPut("checklist/{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleChecklistItem(Guid id, CancellationToken cancellationToken)
    {
        var command = new ToggleChecklistItemCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return NoContent();
    }

    [HttpDelete("checklist/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteChecklistItem(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteChecklistItemCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return NoContent();
    }
}

public record AddChecklistItemRequest(string Text);
