using FGames.Modules.Games.Application.Interfaces;
using FGames.Modules.Promotions.Application.Queries;
using MediatR;

namespace FGames.Api.Adapters;

public sealed class GamesActivePromotionLookupServiceAdapter : IActivePromotionLookupService
{
    private readonly ISender _sender;

    public GamesActivePromotionLookupServiceAdapter(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IReadOnlyDictionary<Guid, ActivePromotionLookupResult>> GetActivePromotionsForGamesAsync(
        IReadOnlyCollection<Guid> gameIds,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetActivePromotionsForGamesQuery(gameIds), cancellationToken);

        if (result.IsFailure)
            return new Dictionary<Guid, ActivePromotionLookupResult>();

        return result.Value.ToDictionary(
            item => item.Key,
            item => new ActivePromotionLookupResult(
                item.Value.PromotionId,
                item.Value.DiscountPercentage));
    }
}
