using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Repositories;
using Modules.Operations.Infrastructure.Persistence;
using Modules.Operations.Infrastructure.Persistence.Repositories;

namespace Modules.Operations.Infrastructure;

public static class OperationsServiceCollectionExtensions
{
    public static IServiceCollection AddOperationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=smartcondo_dev;Username=postgres;Password=postgres";

        services.AddDbContext<OperationsDbContext>((sp, options) =>
        {
            options.UseNpgsqlWithVector(connectionString);
        });

        services.TryAddSingleton<IDistributedLockService, InMemoryDistributedLockService>();

        services.AddScoped<IAreaComumRepository, AreaComumRepository>();
        services.AddScoped<IAreaComumApplicationService, AreaComumApplicationService>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IReservaApplicationService, ReservaApplicationService>();
        services.AddScoped<IOcorrenciaRepository, OcorrenciaRepository>();
        services.AddScoped<IOcorrenciaApplicationService, OcorrenciaApplicationService>();
        services.AddScoped<IPlanoManutencaoApplicationService, PlanoManutencaoApplicationService>();

        return services;
    }
}
