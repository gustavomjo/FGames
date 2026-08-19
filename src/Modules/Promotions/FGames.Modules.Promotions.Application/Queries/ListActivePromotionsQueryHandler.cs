using FGames.Modules.Promotions.Application.DTOs;
using FGames.Modules.Promotions.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Queries;

public sealed class ListActivePromotionsQueryHandler : IRequestHandler<ListActivePromotionsQuery, Result<IReadOnlyList<PromotionDto>>>
{
    private readonly IPromotionRepository _promotionRepository;

    public ListActivePromotionsQueryHandler(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<Result<IReadOnlyList<PromotionDto>>> Handle(
        ListActivePromotionsQuery request,
        CancellationToken cancellationToken)
    {
        var promotions = await _promotionRepository.ListActiveAsync(DateTime.UtcNow, cancellationToken);
        return Result.Success<IReadOnlyList<PromotionDto>>(
            promotions.Select(PromotionDto.FromEntity).ToList());
    }
}
