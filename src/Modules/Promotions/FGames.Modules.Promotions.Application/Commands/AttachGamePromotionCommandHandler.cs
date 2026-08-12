using FGames.Modules.Promotions.Application.DTOs;
using FGames.Modules.Promotions.Application;
using FGames.Modules.Promotions.Application.Interfaces;
using FGames.Modules.Promotions.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Commands;

public sealed class AttachGamePromotionCommandHandler : IRequestHandler<AttachGamePromotionCommand, Result<PromotionDto>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IGameLookupService _gameLookupService;
    private readonly IUnitOfWork _unitOfWork;

    public AttachGamePromotionCommandHandler(
        IPromotionRepository promotionRepository,
        IGameLookupService gameLookupService,
        IUnitOfWork unitOfWork)
    {
        _promotionRepository = promotionRepository;
        _gameLookupService = gameLookupService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PromotionDto>> Handle(AttachGamePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByIdAsync(request.PromotionId, cancellationToken);
        if (promotion is null)
            return Result.Failure<PromotionDto>(new Error("Promotion.NotFound", "Promoção não encontrada."));

        var game = await _gameLookupService.GetPublishedGameAsync(request.GameId, cancellationToken);
        if (game is null)
            return Result.Failure<PromotionDto>(new Error("Promotion.GameNotFound", "Jogo não encontrado ou não publicado."));

        var attachResult = promotion.AttachToGame(request.GameId, request.CreatedByUserId);
        if (attachResult.IsFailure)
            return Result.Failure<PromotionDto>(attachResult.Errors);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(PromotionDto.FromEntity(promotion));
    }
}
