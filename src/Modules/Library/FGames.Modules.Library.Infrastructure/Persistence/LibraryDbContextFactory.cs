using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FGames.Modules.Library.Infrastructure.Persistence;

public sealed class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    public LibraryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=fgames;Username=fgames;Password=fgames_dev_only");
        return new LibraryDbContext(optionsBuilder.Options);
    }
}
