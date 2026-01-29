using FluentValidation;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(TaskItem.TitleMaxLength)
            .WithMessage($"Task title must not exceed {TaskItem.TitleMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(TaskItem.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {TaskItem.DescriptionMaxLength} characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");
    }
}
