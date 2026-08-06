namespace FGames.Modules.Promotions.Application.Interfaces;

public interface IGameLookupService
{
    Task<GameLookupResult?> GetPublishedGameAsync(Guid gameId, CancellationToken cancellationToken = default);
}

public sealed record GameLookupResult(Guid GameId, string Name, decimal Price, bool IsPublished);
