using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Columns.Commands.ReorderColumns;

public record ReorderColumnsCommand(
    Guid ProjectId,
    List<Guid> ColumnIds
) : IRequest<Result>;
