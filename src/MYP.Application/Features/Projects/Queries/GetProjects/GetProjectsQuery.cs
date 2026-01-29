using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;

namespace MYP.Application.Features.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<Result<List<ProjectSummaryDto>>>;
