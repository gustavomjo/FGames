using FGames.Modules.Games.Domain.Entities;
using FGames.Modules.Games.Domain.Enums;

namespace FGames.Modules.Games.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> ListAsync(GameStatus? status = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludedGameId = null, CancellationToken cancellationToken = default);
    void Add(Game game);
}
