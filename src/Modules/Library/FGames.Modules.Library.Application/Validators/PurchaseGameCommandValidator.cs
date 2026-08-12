using FGames.Modules.Library.Application.Commands;
using FluentValidation;

namespace FGames.Modules.Library.Application.Validators;

public sealed class PurchaseGameCommandValidator : AbstractValidator<PurchaseGameCommand>
{
    public PurchaseGameCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.GameId).NotEmpty();
    }
}
