using FGames.Modules.Games.Application.Queries;
using FGames.Modules.Promotions.Application.Interfaces;
using MediatR;

namespace FGames.Api.Adapters;

public sealed class PromotionsGameLookupServiceAdapter : IGameLookupService
{
    private readonly ISender _sender;

    public PromotionsGameLookupServiceAdapter(ISender sender)
    {
        _sender = sender;
    }

    public async Task<GameLookupResult?> GetPublishedGameAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetGameByIdQuery(gameId), cancellationToken);

        return result.IsFailure || result.Value.Status != "Published"
            ? null
            : new GameLookupResult(result.Value.Id, result.Value.Name, result.Value.Price, true);
    }
}
