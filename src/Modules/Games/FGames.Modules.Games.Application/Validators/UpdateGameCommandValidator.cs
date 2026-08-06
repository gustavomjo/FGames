using FGames.Modules.Games.Application.Commands;
using FluentValidation;

namespace FGames.Modules.Games.Application.Validators;

public sealed class UpdateGameCommandValidator : AbstractValidator<UpdateGameCommand>
{
    public UpdateGameCommandValidator()
    {
        RuleFor(command => command.GameId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty();
        RuleFor(command => command.Price).GreaterThanOrEqualTo(0);
    }
}
