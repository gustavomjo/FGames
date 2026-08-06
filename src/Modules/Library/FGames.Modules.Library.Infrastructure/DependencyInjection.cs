using FGames.Modules.Library.Application;
using FGames.Modules.Library.Domain.Interfaces;
using FGames.Modules.Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FGames.Modules.Library.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLibraryInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<LibraryDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LibraryDbContext>());

        services.AddScoped<IUserGameRepository, UserGameRepository>();

        return services;
    }
}
