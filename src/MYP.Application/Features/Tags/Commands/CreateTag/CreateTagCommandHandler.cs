using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Snippets.DTOs;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Tags.Commands.CreateTag;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<TagDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTagCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<TagDto>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result.Failure<TagDto>("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        // Check if tag with same name already exists for this user
        var existingTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.UserId == userId &&
                                      t.Name.ToLower() == request.Name.ToLower(),
                                 cancellationToken);

        if (existingTag is not null)
        {
            return Result.Failure<TagDto>("A tag with this name already exists.");
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Color = request.Color ?? "#6366f1",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new TagDto(tag.Id, tag.Name, tag.Color);
        return Result.Success(response);
    }
}
