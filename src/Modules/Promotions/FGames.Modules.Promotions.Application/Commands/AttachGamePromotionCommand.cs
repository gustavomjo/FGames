using FGames.Modules.Promotions.Application.DTOs;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Commands;

public sealed record AttachGamePromotionCommand(
    int PromotionId,
    Guid GameId,
    Guid CreatedByUserId) : IRequest<Result<PromotionDto>>;
