using FGames.Modules.Library.Application.Interfaces;
using FGames.Modules.Promotions.Application.Queries;
using MediatR;

namespace FGames.Api.Adapters;

public sealed class LibraryActivePromotionLookupServiceAdapter : IActivePromotionLookupService
{
    private readonly ISender _sender;

    public LibraryActivePromotionLookupServiceAdapter(ISender sender)
    {
        _sender = sender;
    }

    public async Task<ActivePromotionLookupResult?> GetActivePromotionForGameAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetActivePromotionForGameQuery(gameId), cancellationToken);

        if (result.IsFailure || result.Value is null)
            return null;

        return new ActivePromotionLookupResult(result.Value.PromotionId, result.Value.DiscountPercentage);
    }
}
