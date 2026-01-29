using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.Application.Features.Tasks.Queries.GetTaskById;

public record GetTaskByIdQuery(Guid Id) : IRequest<Result<TaskItemDto>>;
