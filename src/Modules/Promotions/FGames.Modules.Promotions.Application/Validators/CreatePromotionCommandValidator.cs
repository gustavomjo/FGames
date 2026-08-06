using FGames.Modules.Promotions.Application.Commands;
using FluentValidation;

namespace FGames.Modules.Promotions.Application.Validators;

public sealed class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(command => command.CreatedByUserId).NotEmpty();
        RuleFor(command => command.EndDate).GreaterThan(command => command.StartDate);
        RuleFor(command => command.DiscountPercentage).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
