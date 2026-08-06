using FGames.Modules.Promotions.Application.DTOs;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Queries;

public sealed record ListActivePromotionsQuery : IRequest<Result<IReadOnlyList<PromotionDto>>>;
