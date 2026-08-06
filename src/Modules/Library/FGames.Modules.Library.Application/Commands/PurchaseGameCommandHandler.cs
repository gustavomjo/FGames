using FGames.Modules.Library.Application;
using FGames.Modules.Library.Application.DTOs;
using FGames.Modules.Library.Application.Interfaces;
using FGames.Modules.Library.Domain.Entities;
using FGames.Modules.Library.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Library.Application.Commands;

public sealed class PurchaseGameCommandHandler : IRequestHandler<PurchaseGameCommand, Result<UserGameDto>>
{
    private readonly IUserGameRepository _userGameRepository;
    private readonly IGameLookupService _gameLookupService;
    private readonly IActivePromotionLookupService _activePromotionLookupService;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseGameCommandHandler(
        IUserGameRepository userGameRepository,
        IGameLookupService gameLookupService,
        IActivePromotionLookupService activePromotionLookupService,
        IUnitOfWork unitOfWork)
    {
        _userGameRepository = userGameRepository;
        _gameLookupService = gameLookupService;
        _activePromotionLookupService = activePromotionLookupService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserGameDto>> Handle(PurchaseGameCommand request, CancellationToken cancellationToken)
    {
        if (await _userGameRepository.ExistsAsync(request.UserId, request.GameId, cancellationToken))
            return Result.Failure<UserGameDto>(new Error("UserGame.AlreadyPurchased", "Este jogo já foi adquirido por este usuário."));

        var game = await _gameLookupService.GetPublishedGameAsync(request.GameId, cancellationToken);
        if (game is null)
            return Result.Failure<UserGameDto>(new Error("UserGame.GameNotFound", "Jogo não encontrado ou não publicado."));

        var activePromotion = await _activePromotionLookupService.GetActivePromotionForGameAsync(request.GameId, cancellationToken);

        var pricePaid = activePromotion is null
            ? game.Price
            : game.Price - game.Price * activePromotion.DiscountPercentage / 100m;

        var userGameResult = UserGame.Purchase(request.UserId, request.GameId, pricePaid);
        if (userGameResult.IsFailure)
            return Result.Failure<UserGameDto>(userGameResult.Errors);

        _userGameRepository.Add(userGameResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(UserGameDto.FromEntity(userGameResult.Value));
    }
}
