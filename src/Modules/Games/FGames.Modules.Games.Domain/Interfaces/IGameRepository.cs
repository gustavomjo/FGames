using FGames.Modules.Games.Domain.Entities;

namespace FGames.Modules.Games.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> ListPublishedAsync(CancellationToken cancellationToken = default);
    void Add(Game game);
}
