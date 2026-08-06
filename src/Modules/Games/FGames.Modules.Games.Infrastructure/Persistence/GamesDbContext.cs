using FGames.Modules.Games.Application;
using FGames.Modules.Games.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FGames.Modules.Games.Infrastructure.Persistence;

public sealed class GamesDbContext : DbContext, IUnitOfWork
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("games");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GamesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
