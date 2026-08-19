using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Domain.Enums;
using FGames.Modules.Games.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Queries;

public sealed class GetGameByIdQueryHandler : IRequestHandler<GetGameByIdQuery, Result<GameDto>>
{
    private readonly IGameRepository _gameRepository;

    public GetGameByIdQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<Result<GameDto>> Handle(GetGameByIdQuery request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.Id, cancellationToken);

        return game is null || (!request.IncludeUnpublished && game.Status != GameStatus.Published)
            ? Result.Failure<GameDto>(new Error("Game.NotFound", "Jogo não encontrado."))
            : Result.Success(GameDto.FromEntity(game));
    }
}
