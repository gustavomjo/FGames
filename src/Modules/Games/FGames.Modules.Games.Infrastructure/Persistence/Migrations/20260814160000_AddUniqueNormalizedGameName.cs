using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGames.Modules.Games.Infrastructure.Persistence.Migrations;

[DbContext(typeof(GamesDbContext))]
[Migration("20260814160000_AddUniqueNormalizedGameName")]
public sealed class AddUniqueNormalizedGameName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM games."Games"
                    GROUP BY lower(btrim("Name"))
                    HAVING COUNT(*) > 1
                ) THEN
                    RAISE EXCEPTION 'Existem jogos com nomes duplicados. Execute scripts/find_duplicate_games.sql e corrija os dados antes de aplicar esta migração.';
                END IF;
            END $$;

            CREATE UNIQUE INDEX "uq_game_name_normalized"
                ON games."Games" (lower(btrim("Name")));
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS games.\"uq_game_name_normalized\";");
    }
}
