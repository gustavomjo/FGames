using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Application;
using FGames.Modules.Games.Domain.Entities;
using FGames.Modules.Games.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Commands;

public sealed class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, Result<GameDto>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGameCommandHandler(IGameRepository gameRepository, IUnitOfWork unitOfWork)
    {
        _gameRepository = gameRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GameDto>> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var gameResult = Game.Create(request.Name, request.Description, request.Category, request.Rating, request.Price, request.CreatedByUserId);
        if (gameResult.IsFailure)
            return Result.Failure<GameDto>(gameResult.Errors);

        if (await _gameRepository.ExistsByNameAsync(gameResult.Value.Name, cancellationToken: cancellationToken))
        {
            return Result.Failure<GameDto>(new Error(
                "Game.NameAlreadyExists",
                "Já existe um jogo cadastrado com este nome."));
        }

        _gameRepository.Add(gameResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(GameDto.FromEntity(gameResult.Value));
    }
}
