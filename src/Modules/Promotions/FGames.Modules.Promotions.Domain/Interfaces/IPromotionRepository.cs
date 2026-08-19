using FGames.Modules.Promotions.Domain.Entities;

namespace FGames.Modules.Promotions.Domain.Interfaces;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Promotion>> ListActiveAsync(
        DateTime moment,
        CancellationToken cancellationToken = default);

    Task<Promotion?> GetActiveForGameAsync(
        Guid gameId,
        DateTime moment,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, Promotion>> GetActiveForGamesAsync(
        IReadOnlyCollection<Guid> gameIds,
        DateTime moment,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingActivePromotionAsync(
        Guid gameId,
        DateTime startDate,
        DateTime endDate,
        int excludedPromotionId,
        CancellationToken cancellationToken = default);

    Task AcquireGamePromotionLockAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);

    void Add(Promotion promotion);
}
