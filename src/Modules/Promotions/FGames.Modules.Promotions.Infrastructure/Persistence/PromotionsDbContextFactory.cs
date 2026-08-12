using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FGames.Modules.Promotions.Infrastructure.Persistence;

public sealed class PromotionsDbContextFactory : IDesignTimeDbContextFactory<PromotionsDbContext>
{
    public PromotionsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PromotionsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=fgames;Username=fgames;Password=fgames_dev_only");
        return new PromotionsDbContext(optionsBuilder.Options);
    }
}
