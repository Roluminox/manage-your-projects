using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.Application.Features.Tags.Queries.GetTags;

public record GetTagsQuery : IRequest<Result<IReadOnlyList<TagDto>>>;
