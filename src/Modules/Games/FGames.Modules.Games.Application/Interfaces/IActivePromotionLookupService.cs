namespace FGames.Modules.Games.Application.Interfaces;

public interface IActivePromotionLookupService
{
    Task<ActivePromotionLookupResult?> GetActivePromotionForGameAsync(Guid gameId, CancellationToken cancellationToken = default);
}

public sealed record ActivePromotionLookupResult(int PromotionId, decimal DiscountPercentage);
