using FluentValidation;
using ToDo.Application.Todos.Dtos;

namespace ToDo.Application.Todos.Validators;

public sealed class UpdateTodoRequestValidator : AbstractValidator<UpdateTodoRequest>
{
    public UpdateTodoRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must be 2000 characters or fewer.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be one of Pending, InProgress or Done.");
    }
}
