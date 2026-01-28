using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Auth.DTOs;

namespace MYP.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthResponseDto>>;
