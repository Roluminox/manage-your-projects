using FluentValidation;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Checklists.Commands.AddChecklistItem;

public class AddChecklistItemCommandValidator : AbstractValidator<AddChecklistItemCommand>
{
    public AddChecklistItemCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Checklist item text is required.")
            .MaximumLength(ChecklistItem.TextMaxLength)
            .WithMessage($"Text must not exceed {ChecklistItem.TextMaxLength} characters.");
    }
}
