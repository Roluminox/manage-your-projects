using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (emailExists)
        {
            return Result.Failure<Guid>("An account with this email already exists.");
        }

        // Check if username already exists
        var usernameExists = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == request.Username.ToLower(), cancellationToken);

        if (usernameExists)
        {
            return Result.Failure<Guid>("This username is already taken.");
        }

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
