using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            ?? "Host=localhost;Database=smartcondo;Username=postgres;Password=postgres";

        services.AddDbContext<OperationsDbContext>((sp, options) =>
        {
            options.UseNpgsqlWithVector(connectionString);
        });

        services.AddScoped<IAreaComumRepository, AreaComumRepository>();
        services.AddScoped<IAreaComumApplicationService, AreaComumApplicationService>();

        return services;
    }
}
