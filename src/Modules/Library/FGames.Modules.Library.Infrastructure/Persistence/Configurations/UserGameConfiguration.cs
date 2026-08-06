using FGames.Modules.Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGames.Modules.Library.Infrastructure.Persistence.Configurations;

public sealed class UserGameConfiguration : IEntityTypeConfiguration<UserGame>
{
    public void Configure(EntityTypeBuilder<UserGame> builder)
    {
        builder.ToTable("UserGames");

        builder.HasKey(ug => ug.Id);

        builder.Property(ug => ug.PricePaid).HasColumnType("numeric(18,2)");

        builder.HasIndex(ug => new { ug.UserId, ug.GameId })
            .IsUnique()
            .HasDatabaseName("uq_user_game");
    }
}
