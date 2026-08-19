using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Application.Interfaces;
using FGames.Modules.Games.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Queries;

public sealed class ListGamesQueryHandler : IRequestHandler<ListGamesQuery, Result<IReadOnlyList<GameDto>>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IActivePromotionLookupService _activePromotionLookupService;

    public ListGamesQueryHandler(
        IGameRepository gameRepository,
        IActivePromotionLookupService activePromotionLookupService)
    {
        _gameRepository = gameRepository;
        _activePromotionLookupService = activePromotionLookupService;
    }

    public async Task<Result<IReadOnlyList<GameDto>>> Handle(
        ListGamesQuery request,
        CancellationToken cancellationToken)
    {
        var games = await _gameRepository.ListAsync(request.Status, cancellationToken);
        var gameIds = games.Select(game => game.Id).ToArray();
        var promotions = await _activePromotionLookupService
            .GetActivePromotionsForGamesAsync(gameIds, cancellationToken);

        var dtos = games
            .Select(game =>
            {
                promotions.TryGetValue(game.Id, out var promotion);
                return GameDto.FromEntity(game, promotion?.DiscountPercentage);
            })
            .ToList();

        return Result.Success<IReadOnlyList<GameDto>>(dtos);
    }
}
