using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MYP.Application.Features.Snippets.Commands.CreateSnippet;
using MYP.Application.Features.Snippets.Commands.DeleteSnippet;
using MYP.Application.Features.Snippets.Commands.ToggleFavorite;
using MYP.Application.Features.Snippets.Commands.UpdateSnippet;
using MYP.Application.Features.Snippets.DTOs;
using MYP.Application.Features.Snippets.Queries.GetSnippetById;
using MYP.Application.Features.Snippets.Queries.GetSnippets;
using MYP.Application.Features.Snippets.Queries.SearchSnippets;

namespace MYP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SnippetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SnippetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SnippetListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSnippets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? language = null,
        [FromQuery] Guid? tagId = null,
        [FromQuery] bool? isFavorite = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSnippetsQuery(
            Page: page,
            PageSize: pageSize,
            Language: language,
            TagId: tagId,
            IsFavorite: isFavorite,
            SortBy: sortBy,
            SortDescending: sortDescending
        );

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SnippetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSnippetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSnippetByIdQuery(id);

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(SnippetListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchSnippets(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchSnippetsQuery(
            SearchTerm: q,
            Page: page,
            PageSize: pageSize
        );

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SnippetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSnippet([FromBody] CreateSnippetRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSnippetCommand(
            Title: request.Title,
            Code: request.Code,
            Language: request.Language,
            Description: request.Description,
            TagIds: request.TagIds
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetSnippetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SnippetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSnippet(Guid id, [FromBody] UpdateSnippetRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateSnippetCommand(
            Id: id,
            Title: request.Title,
            Code: request.Code,
            Language: request.Language,
            Description: request.Description,
            TagIds: request.TagIds
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Contains("Snippet not found.")
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSnippet(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteSnippetCommand(id);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/favorite")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken cancellationToken)
    {
        var command = new ToggleFavoriteCommand(id);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return Ok(new { isFavorite = result.Value });
    }
}
