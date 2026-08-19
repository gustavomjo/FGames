using FGames.Modules.Promotions.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Queries;

public sealed class GetActivePromotionsForGamesQueryHandler
    : IRequestHandler<GetActivePromotionsForGamesQuery, Result<IReadOnlyDictionary<Guid, ActivePromotionForGameDto>>>
{
    private readonly IPromotionRepository _promotionRepository;

    public GetActivePromotionsForGamesQueryHandler(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<Result<IReadOnlyDictionary<Guid, ActivePromotionForGameDto>>> Handle(
        GetActivePromotionsForGamesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.GameIds.Count == 0)
        {
            return Result.Success<IReadOnlyDictionary<Guid, ActivePromotionForGameDto>>(
                new Dictionary<Guid, ActivePromotionForGameDto>());
        }

        var promotions = await _promotionRepository.GetActiveForGamesAsync(
            request.GameIds,
            DateTime.UtcNow,
            cancellationToken);

        var result = promotions.ToDictionary(
            item => item.Key,
            item => new ActivePromotionForGameDto(
                item.Value.Id,
                item.Value.DiscountPercentage));

        return Result.Success<IReadOnlyDictionary<Guid, ActivePromotionForGameDto>>(result);
    }
}
