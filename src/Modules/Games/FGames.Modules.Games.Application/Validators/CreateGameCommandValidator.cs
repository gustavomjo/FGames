using FGames.Modules.Games.Application.Commands;
using FluentValidation;

namespace FGames.Modules.Games.Application.Validators;

public sealed class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty();
        RuleFor(command => command.Category).IsInEnum();
        RuleFor(command => command.Rating).IsInEnum();
        RuleFor(command => command.Price).GreaterThanOrEqualTo(0);
        RuleFor(command => command.CreatedByUserId).NotEmpty();
    }
}
