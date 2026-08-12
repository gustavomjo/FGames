using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Queries;

public sealed record GetActivePromotionForGameQuery(Guid GameId) : IRequest<Result<ActivePromotionForGameDto?>>;

public sealed record ActivePromotionForGameDto(int PromotionId, decimal DiscountPercentage);
