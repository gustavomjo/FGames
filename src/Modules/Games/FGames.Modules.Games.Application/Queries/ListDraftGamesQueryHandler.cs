using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Application.Interfaces;
using FGames.Modules.Games.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Queries;

public sealed class ListDraftGamesQueryHandler : IRequestHandler<ListDraftGamesQuery, Result<IReadOnlyList<GameDto>>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IActivePromotionLookupService _activePromotionLookupService;

    public ListDraftGamesQueryHandler(IGameRepository gameRepository, IActivePromotionLookupService activePromotionLookupService)
    {
        _gameRepository = gameRepository;
        _activePromotionLookupService = activePromotionLookupService;
    }

    public async Task<Result<IReadOnlyList<GameDto>>> Handle(ListDraftGamesQuery request, CancellationToken cancellationToken)
    {
        var games = await _gameRepository.ListDraftAsync(cancellationToken);

        var dtos = new List<GameDto>(games.Count);
        foreach (var game in games)
        {
            var promotion = await _activePromotionLookupService.GetActivePromotionForGameAsync(game.Id, cancellationToken);
            dtos.Add(GameDto.FromEntity(game, promotion?.DiscountPercentage));
        }

        return Result.Success<IReadOnlyList<GameDto>>(dtos);
    }
}
