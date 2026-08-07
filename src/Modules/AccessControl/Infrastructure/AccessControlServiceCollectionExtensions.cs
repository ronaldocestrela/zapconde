using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.AccessControl.Application.Services;
using Modules.AccessControl.Infrastructure.Persistence;

namespace Modules.AccessControl.Infrastructure;

public static class AccessControlServiceCollectionExtensions
{
    public static IServiceCollection AddAccessControlModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=smartcondo_dev;Username=postgres;Password=postgres";

        services.AddDbContext<AccessControlDbContext>((sp, options) =>
        {
            options.UseNpgsqlWithVector(connectionString);
        });

        services.AddScoped<IVisitanteApplicationService, VisitanteApplicationService>();
        services.AddScoped<IEncomendaApplicationService, EncomendaApplicationService>();

        return services;
    }
}
