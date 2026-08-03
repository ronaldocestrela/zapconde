using FastEndpoints;

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
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // FastEndpoints para gerenciamento de endpoints
        services.AddFastEndpoints();

        return services;
    }

    /// <summary>
    /// Configura documentação OpenAPI
    /// (será expandido na Subfase 1.1.3)
    /// </summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();

        return services;
    }
}
