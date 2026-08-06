using FGames.Modules.Library.Domain.Entities;

namespace FGames.Modules.Library.Domain.Interfaces;

public interface IUserGameRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserGame>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(UserGame userGame);
}
