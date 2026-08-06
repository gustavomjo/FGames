using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Queries;

public sealed class ListPublishedGamesQueryHandler : IRequestHandler<ListPublishedGamesQuery, Result<IReadOnlyList<GameDto>>>
{
    private readonly IGameRepository _gameRepository;

    public ListPublishedGamesQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<Result<IReadOnlyList<GameDto>>> Handle(ListPublishedGamesQuery request, CancellationToken cancellationToken)
    {
        var games = await _gameRepository.ListPublishedAsync(cancellationToken);
        return Result.Success<IReadOnlyList<GameDto>>(games.Select(GameDto.FromEntity).ToList());
    }
}
