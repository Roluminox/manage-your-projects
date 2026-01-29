namespace MYP.Application.Features.Snippets.DTOs;

public record SnippetDto(
    Guid Id,
    string Title,
    string Code,
    string Language,
    string? Description,
    bool IsFavorite,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagDto> Tags
);

public record SnippetSummaryDto(
    Guid Id,
    string Title,
    string Language,
    bool IsFavorite,
    DateTime CreatedAt,
    IReadOnlyList<TagDto> Tags
);

public record TagDto(
    Guid Id,
    string Name,
    string Color
);

public record CreateSnippetRequest(
    string Title,
    string Code,
    string Language,
    string? Description,
    List<Guid>? TagIds
);

public record UpdateSnippetRequest(
    string Title,
    string Code,
    string Language,
    string? Description,
    List<Guid>? TagIds
);

public record SnippetListResponse(
    IReadOnlyList<SnippetSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
