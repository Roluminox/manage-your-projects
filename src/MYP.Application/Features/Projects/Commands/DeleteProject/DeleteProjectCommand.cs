using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid Id) : IRequest<Result>;
