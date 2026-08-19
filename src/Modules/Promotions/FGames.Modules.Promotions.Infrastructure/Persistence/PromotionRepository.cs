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
            .Include(promotion => promotion.GamePromotions)
            .FirstOrDefaultAsync(promotion => promotion.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Promotion>> ListActiveAsync(
        DateTime moment,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Promotions
            .AsNoTracking()
            .Include(promotion => promotion.GamePromotions)
            .Where(promotion =>
                promotion.Active &&
                promotion.StartDate <= moment &&
                promotion.EndDate >= moment)
            .OrderBy(promotion => promotion.EndDate)
            .ToListAsync(cancellationToken);

    public Task<Promotion?> GetActiveForGameAsync(
        Guid gameId,
        DateTime moment,
        CancellationToken cancellationToken = default) =>
        _dbContext.Promotions
            .AsNoTracking()
            .Where(promotion =>
                promotion.Active &&
                promotion.StartDate <= moment &&
                promotion.EndDate >= moment &&
                promotion.GamePromotions.Any(gamePromotion => gamePromotion.GameId == gameId))
            .OrderByDescending(promotion => promotion.DiscountPercentage)
            .ThenBy(promotion => promotion.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, Promotion>> GetActiveForGamesAsync(
        IReadOnlyCollection<Guid> gameIds,
        DateTime moment,
        CancellationToken cancellationToken = default)
    {
        if (gameIds.Count == 0)
            return new Dictionary<Guid, Promotion>();

        var matches = await (
                from gamePromotion in _dbContext.GamePromotions.AsNoTracking()
                join promotion in _dbContext.Promotions.AsNoTracking()
                    on gamePromotion.PromotionId equals promotion.Id
                where gameIds.Contains(gamePromotion.GameId)
                      && promotion.Active
                      && promotion.StartDate <= moment
                      && promotion.EndDate >= moment
                select new { gamePromotion.GameId, Promotion = promotion })
            .ToListAsync(cancellationToken);

        return matches
            .GroupBy(match => match.GameId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(match => match.Promotion.DiscountPercentage)
                    .ThenBy(match => match.Promotion.Id)
                    .First()
                    .Promotion);
    }

    public Task<bool> HasOverlappingActivePromotionAsync(
        Guid gameId,
        DateTime startDate,
        DateTime endDate,
        int excludedPromotionId,
        CancellationToken cancellationToken = default) =>
        (
            from gamePromotion in _dbContext.GamePromotions
            join promotion in _dbContext.Promotions
                on gamePromotion.PromotionId equals promotion.Id
            where gamePromotion.GameId == gameId
                  && promotion.Id != excludedPromotionId
                  && promotion.Active
                  && promotion.StartDate <= endDate
                  && startDate <= promotion.EndDate
            select promotion)
        .AnyAsync(cancellationToken);

    public async Task AcquireGamePromotionLockAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var bytes = gameId.ToByteArray();
        var firstKey = BitConverter.ToInt32(bytes, 0);
        var secondKey = BitConverter.ToInt32(bytes, 4);

        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({firstKey}, {secondKey})",
            cancellationToken);
    }

    public void Add(Promotion promotion) => _dbContext.Promotions.Add(promotion);
}
