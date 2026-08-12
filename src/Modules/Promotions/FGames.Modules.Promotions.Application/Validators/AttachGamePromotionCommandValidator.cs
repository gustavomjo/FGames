using FGames.Modules.Promotions.Application.Commands;
using FluentValidation;

namespace FGames.Modules.Promotions.Application.Validators;

public sealed class AttachGamePromotionCommandValidator : AbstractValidator<AttachGamePromotionCommand>
{
    public AttachGamePromotionCommandValidator()
    {
        RuleFor(command => command.PromotionId).GreaterThan(0);
        RuleFor(command => command.GameId).NotEmpty();
        RuleFor(command => command.CreatedByUserId).NotEmpty();
    }
}
