using FGames.Modules.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGames.Modules.Promotions.Infrastructure.Persistence.Configurations;

public sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions", table =>
        {
            table.HasCheckConstraint("chk_promotion_valid_period", "\"EndDate\" > \"StartDate\"");
            table.HasCheckConstraint("chk_promotion_valid_percentage", "\"DiscountPercentage\" > 0 AND \"DiscountPercentage\" <= 100");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.DiscountPercentage).HasColumnType("numeric(5,2)");

        builder.HasMany(p => p.GamePromotions)
            .WithOne()
            .HasForeignKey(gp => gp.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.GamePromotions)
            .HasField("_gamePromotions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
