using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Projects.DTOs;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Columns.Commands.CreateColumn;

public class CreateColumnCommandHandler : IRequestHandler<CreateColumnCommand, Result<ColumnDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateColumnCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ColumnDto>> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<ColumnDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var project = await _context.Projects
            .Include(p => p.Columns)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.UserId == userId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ColumnDto>("Project not found.");
        }

        var maxOrder = project.Columns.Any()
            ? project.Columns.Max(c => c.Order)
            : -1;

        var column = new Column
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Order = maxOrder + 1,
            ProjectId = project.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Columns.Add(column);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new ColumnDto(
            Id: column.Id,
            Name: column.Name,
            Order: column.Order,
            Tasks: new List<TaskItemDto>()
        );

        return Result.Success(response);
    }
}
