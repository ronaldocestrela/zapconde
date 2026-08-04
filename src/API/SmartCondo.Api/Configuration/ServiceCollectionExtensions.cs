using FastEndpoints;
using BuildingBlocks.Infrastructure.DependencyInjection;
using Modules.Identity.Infrastructure;

namespace SmartCondo.Api.Configuration;

/// <summary>
/// Extensões para configuração de serviços da aplicação.
/// Facilita a manutenção e separação de responsabilidades do bootstrap.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura os serviços principais da API
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddIdentityModule(configuration);

        services.AddFastEndpoints(config =>
        {
            config.Assemblies =
            [
                typeof(Program).Assembly,
                typeof(Modules.Identity.Endpoints.LoginEndpoint).Assembly
            ];
        });

        return services;
    }

    /// <summary>
    /// Configura documentação OpenAPI
    /// (será expandido na Subfase 1.1.3)
    /// </summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi("v1");

        return services;
    }
}
