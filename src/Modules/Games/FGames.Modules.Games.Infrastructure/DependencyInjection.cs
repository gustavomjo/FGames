using FGames.Modules.Games.Application;
using FGames.Modules.Games.Domain.Interfaces;
using FGames.Modules.Games.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FGames.Modules.Games.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGamesInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<GamesDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<GamesDbContext>());

        services.AddScoped<IGameRepository, GameRepository>();

        return services;
    }
}
