using FGames.Modules.Library.Domain.Entities;
using FGames.Modules.Library.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FGames.Modules.Library.Infrastructure.Persistence;

public sealed class UserGameRepository : IUserGameRepository
{
    private readonly LibraryDbContext _dbContext;

    public UserGameRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default) =>
        _dbContext.UserGames.AnyAsync(ug => ug.UserId == userId && ug.GameId == gameId, cancellationToken);

    public async Task<IReadOnlyList<UserGame>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _dbContext.UserGames.Where(ug => ug.UserId == userId).ToListAsync(cancellationToken);

    public void Add(UserGame userGame) => _dbContext.UserGames.Add(userGame);
}
