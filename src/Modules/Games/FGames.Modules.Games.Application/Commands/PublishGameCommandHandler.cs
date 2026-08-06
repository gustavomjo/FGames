using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Application;
using FGames.Modules.Games.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Commands;

public sealed class PublishGameCommandHandler : IRequestHandler<PublishGameCommand, Result<GameDto>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishGameCommandHandler(IGameRepository gameRepository, IUnitOfWork unitOfWork)
    {
        _gameRepository = gameRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GameDto>> Handle(PublishGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.GameId, cancellationToken);
        if (game is null)
            return Result.Failure<GameDto>(new Error("Game.NotFound", "Jogo não encontrado."));

        var publishResult = game.Publish();
        if (publishResult.IsFailure)
            return Result.Failure<GameDto>(publishResult.Errors);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(GameDto.FromEntity(game));
    }
}
