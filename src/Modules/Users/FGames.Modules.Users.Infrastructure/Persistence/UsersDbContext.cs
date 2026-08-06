using FGames.Modules.Users.Application;
using FGames.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FGames.Modules.Users.Infrastructure.Persistence;

public sealed class UsersDbContext : DbContext, IUnitOfWork
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
