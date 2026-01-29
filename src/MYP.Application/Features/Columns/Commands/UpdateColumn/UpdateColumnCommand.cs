using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.Application.Features.Columns.Commands.UpdateColumn;

public record UpdateColumnCommand(
    Guid Id,
    string Name
) : IRequest<Result<ColumnDto>>;
