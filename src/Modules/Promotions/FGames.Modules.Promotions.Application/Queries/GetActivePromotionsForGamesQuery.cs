using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Queries;

public sealed record GetActivePromotionsForGamesQuery(IReadOnlyCollection<Guid> GameIds)
    : IRequest<Result<IReadOnlyDictionary<Guid, ActivePromotionForGameDto>>>;
