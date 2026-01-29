using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.Application.Features.Checklists.Commands.AddChecklistItem;

public record AddChecklistItemCommand(
    Guid TaskId,
    string Text
) : IRequest<Result<ChecklistItemDto>>;
