using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.WhatsApp.Application.Services;
using Modules.WhatsApp.Infrastructure.Persistence;

namespace Modules.WhatsApp.Infrastructure;

public static class WhatsAppServiceCollectionExtensions
{
    public static IServiceCollection AddWhatsAppModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=smartcondo_dev;Username=postgres;Password=postgres";

        services.AddDbContext<WhatsAppDbContext>((sp, options) =>
        {
            options.UseNpgsqlWithVector(connectionString);
        });

        services.AddSingleton<IEvolutionPayloadParser, EvolutionPayloadParser>();
        services.AddScoped<IWhatsAppApplicationService, WhatsAppApplicationService>();

        return services;
    }
}
