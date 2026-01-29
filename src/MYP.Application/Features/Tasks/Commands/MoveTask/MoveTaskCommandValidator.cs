using FluentValidation;

namespace MYP.Application.Features.Tasks.Commands.MoveTask;

public class MoveTaskCommandValidator : AbstractValidator<MoveTaskCommand>
{
    public MoveTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.TargetColumnId)
            .NotEmpty().WithMessage("Target column ID is required.");

        RuleFor(x => x.NewOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be a non-negative number.");
    }
}
