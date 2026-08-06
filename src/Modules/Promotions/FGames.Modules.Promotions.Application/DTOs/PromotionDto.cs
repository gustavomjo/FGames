using FGames.Modules.Promotions.Domain.Entities;

namespace FGames.Modules.Promotions.Application.DTOs;

public sealed record PromotionDto(
    int Id,
    DateTime StartDate,
    DateTime EndDate,
    decimal DiscountPercentage,
    bool Active,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    IReadOnlyCollection<Guid> GameIds)
{
    public static PromotionDto FromEntity(Promotion promotion) => new(
        promotion.Id,
        promotion.StartDate,
        promotion.EndDate,
        promotion.DiscountPercentage,
        promotion.Active,
        promotion.CreatedByUserId,
        promotion.CreatedAt,
        promotion.GamePromotions.Select(gp => gp.GameId).ToList());
}
