using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Username,
    string Password,
    string DisplayName
) : IRequest<Result<Guid>>;
