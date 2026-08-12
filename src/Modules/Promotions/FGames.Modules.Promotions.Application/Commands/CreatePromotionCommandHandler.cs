using FGames.Modules.Promotions.Application.DTOs;
using FGames.Modules.Promotions.Application;
using FGames.Modules.Promotions.Domain.Entities;
using FGames.Modules.Promotions.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Promotions.Application.Commands;

public sealed class CreatePromotionCommandHandler : IRequestHandler<CreatePromotionCommand, Result<PromotionDto>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePromotionCommandHandler(IPromotionRepository promotionRepository, IUnitOfWork unitOfWork)
    {
        _promotionRepository = promotionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PromotionDto>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotionResult = Promotion.Create(request.StartDate, request.EndDate, request.DiscountPercentage, request.CreatedByUserId);
        if (promotionResult.IsFailure)
            return Result.Failure<PromotionDto>(promotionResult.Errors);

        _promotionRepository.Add(promotionResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(PromotionDto.FromEntity(promotionResult.Value));
    }
}
