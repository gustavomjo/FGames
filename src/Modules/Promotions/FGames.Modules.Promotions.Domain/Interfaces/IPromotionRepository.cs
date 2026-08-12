using FGames.Modules.Promotions.Domain.Entities;

namespace FGames.Modules.Promotions.Domain.Interfaces;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Promotion>> ListActiveAsync(CancellationToken cancellationToken = default);
    Task<Promotion?> GetActiveForGameAsync(Guid gameId, DateTime moment, CancellationToken cancellationToken = default);
    void Add(Promotion promotion);
}
