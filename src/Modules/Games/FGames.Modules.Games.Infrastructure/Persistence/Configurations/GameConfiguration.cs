using FGames.Modules.Games.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGames.Modules.Games.Infrastructure.Persistence.Configurations;

public sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(g => g.Description).HasMaxLength(1000);

        builder.Property(g => g.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(g => g.Rating).HasConversion<string>().HasMaxLength(20);
        builder.Property(g => g.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(g => g.Price).HasColumnType("numeric(18,2)");

        builder.HasIndex(g => g.Name).HasDatabaseName("idx_game_name");
        builder.HasIndex(g => g.Status).HasDatabaseName("idx_game_status");
    }
}
