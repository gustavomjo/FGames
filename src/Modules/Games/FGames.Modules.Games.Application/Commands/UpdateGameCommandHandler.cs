using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Application;
using FGames.Modules.Games.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Commands;

public sealed class UpdateGameCommandHandler : IRequestHandler<UpdateGameCommand, Result<GameDto>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGameCommandHandler(IGameRepository gameRepository, IUnitOfWork unitOfWork)
    {
        _gameRepository = gameRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GameDto>> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.GameId, cancellationToken);
        if (game is null)
            return Result.Failure<GameDto>(new Error("Game.NotFound", "Jogo não encontrado."));

        var updateResult = game.Update(request.Name, request.Description, request.Category, request.Rating, request.Price);
        if (updateResult.IsFailure)
            return Result.Failure<GameDto>(updateResult.Errors);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(GameDto.FromEntity(game));
    }
}
