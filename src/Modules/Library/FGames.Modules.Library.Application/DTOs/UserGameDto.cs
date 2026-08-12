using FGames.Modules.Library.Domain.Entities;

namespace FGames.Modules.Library.Application.DTOs;

public sealed record UserGameDto(Guid Id, Guid UserId, Guid GameId, decimal PricePaid, DateTime CreatedAt)
{
    public static UserGameDto FromEntity(UserGame userGame) => new(
        userGame.Id,
        userGame.UserId,
        userGame.GameId,
        userGame.PricePaid,
        userGame.CreatedAt);
}
