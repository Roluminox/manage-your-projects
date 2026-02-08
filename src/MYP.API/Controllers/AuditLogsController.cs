using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MYP.Application.Features.AuditLogs.DTOs;
using MYP.Application.Features.AuditLogs.Queries.GetAuditLogs;
using MYP.Application.Features.AuditLogs.Queries.GetAuditLogsByEntity;
using MYP.Domain.Enums;

namespace MYP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AuditLogListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? entityType = null,
        [FromQuery] AuditAction? action = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery(
            Page: page,
            PageSize: pageSize,
            EntityType: entityType,
            Action: action,
            FromDate: fromDate,
            ToDate: toDate
        );

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpGet("{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditLogsByEntity(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsByEntityQuery(entityType, entityId);

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }
}
