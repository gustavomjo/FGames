using FGames.Modules.Games.Domain.Entities;
using FGames.Modules.Games.Domain.Enums;
using FGames.Modules.Games.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FGames.Modules.Games.Infrastructure.Persistence;

public sealed class GameRepository : IGameRepository
{
    private readonly GamesDbContext _dbContext;

    public GameRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Games.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Game>> ListAsync(
        GameStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Games.AsNoTracking();

        if (status.HasValue)
            query = query.Where(game => game.Status == status.Value);

        return await query
            .OrderBy(game => game.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludedGameId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        return _dbContext.Games.AnyAsync(
            game => (!excludedGameId.HasValue || game.Id != excludedGameId.Value)
                && game.Name.Trim().ToLower() == normalizedName,
            cancellationToken);
    }

    public void Add(Game game) => _dbContext.Games.Add(game);
}
