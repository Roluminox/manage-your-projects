using MediatR;
using MYP.Application.Common.Models;
using MYP.Application.Features.Auth.DTOs;

namespace MYP.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<Result<UserDto>>;
