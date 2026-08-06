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
    Guid CreatedByUserId,
    DateTime CreatedAt)
{
    public static GameDto FromEntity(Game game) => new(
        game.Id,
        game.Name,
        game.Description,
        game.Category.ToString(),
        game.Rating.ToString(),
        game.Status.ToString(),
        game.Price,
        game.CreatedByUserId,
        game.CreatedAt);
}
