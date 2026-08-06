using FGames.Modules.Promotions.Application.DTOs;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Commands;

public sealed record CreatePromotionCommand(
    DateTime StartDate,
    DateTime EndDate,
    decimal DiscountPercentage,
    Guid CreatedByUserId) : IRequest<Result<PromotionDto>>;
