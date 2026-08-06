using FGames.Modules.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGames.Modules.Promotions.Infrastructure.Persistence.Configurations;

public sealed class GamePromotionConfiguration : IEntityTypeConfiguration<GamePromotion>
{
    public void Configure(EntityTypeBuilder<GamePromotion> builder)
    {
        builder.ToTable("GamePromotions");

        builder.HasKey(gp => gp.Id);
        builder.Property(gp => gp.Id).ValueGeneratedOnAdd();

        builder.HasIndex(gp => new { gp.GameId, gp.PromotionId })
            .IsUnique()
            .HasDatabaseName("uq_game_promotion");
    }
}
