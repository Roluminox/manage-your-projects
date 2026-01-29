using MediatR;
using MYP.Application.Common.Models;

namespace MYP.Application.Features.Tags.Commands.DeleteTag;

public record DeleteTagCommand(Guid Id) : IRequest<Result>;
