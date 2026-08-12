using FGames.Modules.Games.Application.Queries;
using FGames.Modules.Library.Application.Interfaces;
using MediatR;

namespace FGames.Api.Adapters;

public sealed class LibraryGameLookupServiceAdapter : IGameLookupService
{
    private readonly ISender _sender;

    public LibraryGameLookupServiceAdapter(ISender sender)
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
