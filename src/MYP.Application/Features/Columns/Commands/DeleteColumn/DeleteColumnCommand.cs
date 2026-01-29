using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Columns.Commands.DeleteColumn;

public record DeleteColumnCommand(Guid Id) : IRequest<Result>;
