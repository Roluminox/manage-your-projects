using MediatR;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateProjectCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<ProjectDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var project = Project.Create(
            request.Name,
            request.Description,
            request.Color,
            userId
        );

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new ProjectDto(
            Id: project.Id,
            Name: project.Name,
            Description: project.Description,
            Color: project.Color,
            CreatedAt: project.CreatedAt,
            UpdatedAt: project.UpdatedAt,
            Columns: new List<ColumnDto>()
        );

        return Result.Success(response);
    }
}
