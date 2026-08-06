using FGames.Modules.Promotions.Application;
using FGames.Modules.Promotions.Domain.Interfaces;
using FGames.Modules.Promotions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FGames.Modules.Promotions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPromotionsInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PromotionsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PromotionsDbContext>());

        services.AddScoped<IPromotionRepository, PromotionRepository>();

        return services;
    }
}
