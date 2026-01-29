using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Tasks.Commands.DeleteTask;

public record DeleteTaskCommand(Guid Id) : IRequest<Result>;
