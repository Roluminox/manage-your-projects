using MYP.Domain.Enums;

namespace MYP.Application.Features.Projects.DTOs;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string Color,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ColumnDto> Columns
);

public record ProjectSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    string Color,
    DateTime CreatedAt,
    int ColumnCount,
    int TaskCount
);

public record ColumnDto(
    Guid Id,
    string Name,
    int Order,
    IReadOnlyList<TaskItemDto> Tasks
);

public record TaskItemDto(
    Guid Id,
    string Title,
    string? Description,
    Priority Priority,
    DateTime? DueDate,
    int Order,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<LabelDto> Labels,
    IReadOnlyList<ChecklistItemDto> Checklists
);

public record TaskItemSummaryDto(
    Guid Id,
    string Title,
    Priority Priority,
    DateTime? DueDate,
    int Order,
    bool IsArchived,
    int LabelCount,
    int ChecklistTotal,
    int ChecklistCompleted
);

public record LabelDto(
    Guid Id,
    string Name,
    string Color
);

public record ChecklistItemDto(
    Guid Id,
    string Text,
    bool IsCompleted,
    int Order
);

public record CreateProjectRequest(
    string Name,
    string? Description,
    string? Color
);

public record UpdateProjectRequest(
    string Name,
    string? Description,
    string? Color
);
