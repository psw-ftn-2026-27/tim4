using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Positions.Application;
using Positions.Application.PositionSimulation;
using Positions.Application.TouristPositions;
using Positions.Infrastructure.Persistence;
using Positions.Infrastructure.Persistence.TouristPositions;
using Shared.Infrastructure;

namespace Positions.Infrastructure;

public static class PositionsModuleExtensions
{
    public static IServiceCollection AddPositionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PositionsDbContext>(configuration, "positions");
        services.AddHostedService<PositionsModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PositionsMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PositionsDbContext>());
        services.AddScoped<ITouristPositionRepository, TouristPositionRepository>();
        services.AddScoped<ITouristPositionReadRepository, TouristPositionReadRepository>();

        services.AddScoped<PositionSimulationService>();
        services.AddScoped<PositionSimulationQueries>();

        return services;
    }
}
