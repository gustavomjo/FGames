using FGames.Modules.Promotions.Domain.Entities;
using FGames.Modules.Promotions.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FGames.Modules.Promotions.Infrastructure.Persistence;

public sealed class PromotionRepository : IPromotionRepository
{
    private readonly PromotionsDbContext _dbContext;

    public PromotionRepository(PromotionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Promotion?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Promotions
            .Include(p => p.GamePromotions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Promotion>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Promotions
            .Include(p => p.GamePromotions)
            .Where(p => p.Active)
            .ToListAsync(cancellationToken);

    public Task<Promotion?> GetActiveForGameAsync(Guid gameId, DateTime moment, CancellationToken cancellationToken = default) =>
        _dbContext.Promotions
            .Include(p => p.GamePromotions)
            .Where(p => p.Active && p.StartDate <= moment && p.EndDate >= moment)
            .FirstOrDefaultAsync(p => p.GamePromotions.Any(gp => gp.GameId == gameId), cancellationToken);

    public void Add(Promotion promotion) => _dbContext.Promotions.Add(promotion);
}
