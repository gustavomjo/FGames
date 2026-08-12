using FGames.Modules.Users.Application.Commands;
using FluentValidation;

namespace FGames.Modules.Users.Application.Validators;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty();
        RuleFor(command => command.Email).NotEmpty();
        RuleFor(command => command.Password).NotEmpty();
        RuleFor(command => command.CreatedByUserId).NotEmpty();
    }
}
