namespace FGames.Modules.Games.Application.Interfaces;

public interface IActivePromotionLookupService
{
    Task<IReadOnlyDictionary<Guid, ActivePromotionLookupResult>> GetActivePromotionsForGamesAsync(
        IReadOnlyCollection<Guid> gameIds,
        CancellationToken cancellationToken = default);
}

public sealed record ActivePromotionLookupResult(int PromotionId, decimal DiscountPercentage);
