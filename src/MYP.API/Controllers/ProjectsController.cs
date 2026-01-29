using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MYP.Application.Features.Projects.Commands.CreateProject;
using MYP.Application.Features.Projects.Commands.DeleteProject;
using MYP.Application.Features.Projects.Commands.UpdateProject;
using MYP.Application.Features.Projects.DTOs;
using MYP.Application.Features.Projects.Queries.GetProjectById;
using MYP.Application.Features.Projects.Queries.GetProjects;

namespace MYP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProjects(CancellationToken cancellationToken)
    {
        var query = new GetProjectsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProjectByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(
            Name: request.Name,
            Description: request.Description,
            Color: request.Color
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetProjectById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand(
            Id: id,
            Name: request.Name,
            Description: request.Description,
            Color: request.Color
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Contains("Project not found.")
                ? NotFound(new { errors = result.Errors })
                : BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProjectCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { errors = result.Errors });
        }

        return NoContent();
    }
}
