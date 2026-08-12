using FGames.Modules.Promotions.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Queries;

public sealed class GetActivePromotionForGameQueryHandler
    : IRequestHandler<GetActivePromotionForGameQuery, Result<ActivePromotionForGameDto?>>
{
    private readonly IPromotionRepository _promotionRepository;

    public GetActivePromotionForGameQueryHandler(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<Result<ActivePromotionForGameDto?>> Handle(GetActivePromotionForGameQuery request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetActiveForGameAsync(request.GameId, DateTime.UtcNow, cancellationToken);

        var dto = promotion is null
            ? null
            : new ActivePromotionForGameDto(promotion.Id, promotion.DiscountPercentage);

        return Result.Success(dto);
    }
}
