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

    public async Task<IReadOnlyList<Game>> ListPublishedAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Games.Where(g => g.Status == GameStatus.Published).ToListAsync(cancellationToken);

    public void Add(Game game) => _dbContext.Games.Add(game);
}
