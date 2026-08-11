using FGames.Modules.Games.Domain.Entities;

namespace FGames.Modules.Games.Application.DTOs;

public sealed record GameDto(
    Guid Id,
    string Name,
    string? Description,
    string Category,
    string Rating,
    string Status,
    decimal Price,
    decimal FinalPrice,
    decimal? DiscountPercentage,
    Guid CreatedByUserId,
    DateTime CreatedAt)
{
    public static GameDto FromEntity(Game game, decimal? discountPercentage = null)
    {
        var finalPrice = discountPercentage is > 0
            ? game.Price - game.Price * discountPercentage.Value / 100m
            : game.Price;

        return new(
            game.Id,
            game.Name,
            game.Description,
            game.Category.ToString(),
            game.Rating.ToString(),
            game.Status.ToString(),
            game.Price,
            finalPrice,
            discountPercentage,
            game.CreatedByUserId,
            game.CreatedAt);
    }
}
