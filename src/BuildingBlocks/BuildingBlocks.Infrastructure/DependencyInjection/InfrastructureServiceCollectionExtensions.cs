using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.DependencyInjection;

/// <summary>
/// Extensões de composição da camada de infraestrutura.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registra serviços de infraestrutura e validações básicas de persistência.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var postgresConnectionString = configuration.GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(postgresConnectionString))
        {
            throw new InvalidOperationException("Connection string 'ConnectionStrings:Postgres' não foi configurada.");
        }

        return services;
    }
}
