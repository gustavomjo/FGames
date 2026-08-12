using FGames.Modules.Users.Domain.Entities;
using FGames.Modules.Users.Domain.Interfaces;
using FGames.Modules.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FGames.Modules.Users.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly UsersDbContext _dbContext;

    public UserRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return Task.FromResult<User?>(null);

        return _dbContext.Users.FirstOrDefaultAsync(u => u.Email == emailResult.Value, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return Task.FromResult(false);

        return _dbContext.Users.AnyAsync(u => u.Email == emailResult.Value, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Users.ToListAsync(cancellationToken);

    public void Add(User user) => _dbContext.Users.Add(user);
}
