using FluentValidation;

namespace MYP.Application.Features.Tasks.Commands.ReorderTasks;

public class ReorderTasksCommandValidator : AbstractValidator<ReorderTasksCommand>
{
    public ReorderTasksCommandValidator()
    {
        RuleFor(x => x.ColumnId)
            .NotEmpty().WithMessage("Column ID is required.");

        RuleFor(x => x.TaskIds)
            .NotEmpty().WithMessage("Task IDs are required.");
    }
}
