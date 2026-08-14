using FGames.Modules.Users.Application.Commands;
using FluentValidation;

namespace FGames.Modules.Users.Application.Validators;

public sealed class SetUserStatusCommandValidator : AbstractValidator<SetUserStatusCommand>
{
    public SetUserStatusCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Status).IsInEnum();
    }
}
