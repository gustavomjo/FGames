using FGames.Modules.Promotions.Application;
using FGames.Modules.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FGames.Modules.Promotions.Infrastructure.Persistence;

public sealed class PromotionsDbContext : DbContext, IUnitOfWork
{
    public PromotionsDbContext(DbContextOptions<PromotionsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<GamePromotion> GamePromotions => Set<GamePromotion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("promotions");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PromotionsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
