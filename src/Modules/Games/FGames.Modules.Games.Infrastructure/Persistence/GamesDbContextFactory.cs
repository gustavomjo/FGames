using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FGames.Modules.Games.Infrastructure.Persistence;

public sealed class GamesDbContextFactory : IDesignTimeDbContextFactory<GamesDbContext>
{
    public GamesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GamesDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=fgames;Username=fgames;Password=fgames_dev_only");
        return new GamesDbContext(optionsBuilder.Options);
    }
}
