using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.Application.Features.Columns.Commands.CreateColumn;

public record CreateColumnCommand(
    Guid ProjectId,
    string Name
) : IRequest<Result<ColumnDto>>;
