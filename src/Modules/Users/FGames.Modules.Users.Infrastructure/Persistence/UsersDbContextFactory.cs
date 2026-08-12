using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FGames.Modules.Users.Infrastructure.Persistence;

public sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=fgames;Username=fgames;Password=fgames_dev_only");
        return new UsersDbContext(optionsBuilder.Options);
    }
}
