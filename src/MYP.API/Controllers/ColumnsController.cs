using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MYP.Application.Features.Columns.Commands.CreateColumn;
using MYP.Application.Features.Columns.Commands.DeleteColumn;
using MYP.Application.Features.Columns.Commands.ReorderColumns;
using MYP.Application.Features.Columns.Commands.UpdateColumn;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.API.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class ColumnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ColumnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("projects/{projectId:guid}/columns")]
    [ProducesResponseType(typeof(ColumnDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateColumn(Guid projectId, [FromBody] CreateColumnRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateColumnCommand(
            ProjectId: projectId,
            Name: request.Name
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Contains("Project not found.")
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return Created($"/api/columns/{result.Value.Id}", result.Value);
    }

    [HttpPut("columns/{id:guid}")]
    [ProducesResponseType(typeof(ColumnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateColumn(Guid id, [FromBody] UpdateColumnRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateColumnCommand(
            Id: id,
            Name: request.Name
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Contains("Column not found.")
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpDelete("columns/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteColumn(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteColumnCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return NoContent();
    }

    [HttpPut("projects/{projectId:guid}/columns/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderColumns(Guid projectId, [FromBody] ReorderColumnsRequest request, CancellationToken cancellationToken)
    {
        var command = new ReorderColumnsCommand(
            ProjectId: projectId,
            ColumnIds: request.ColumnIds
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Contains("Project not found.")
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }
}

public record CreateColumnRequest(string Name);
public record UpdateColumnRequest(string Name);
public record ReorderColumnsRequest(List<Guid> ColumnIds);
