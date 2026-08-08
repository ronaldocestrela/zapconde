using FastEndpoints;
using BuildingBlocks.Infrastructure.DependencyInjection;
using MassTransit;
using Modules.Identity.Infrastructure;
using Modules.Financial.Infrastructure;
using Modules.Operations.Infrastructure;
using Modules.AccessControl.Infrastructure;
using Modules.WhatsApp.Infrastructure;
using Modules.WhatsApp.Infrastructure.Persistence;
using Modules.AIEngine.Infrastructure;

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
        services.AddInfrastructure(configuration, busConfigurator =>
        {
            busConfigurator.AddConsumer<Modules.WhatsApp.Application.Consumers.WhatsAppInboundConsumer>();
            busConfigurator.AddConsumer<BuildingBlocks.Infrastructure.Email.SendEmailConsumer>();

            busConfigurator.AddEntityFrameworkOutbox<WhatsAppDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
        });
        services.AddIdentityModule(configuration);
        services.AddFinancialModule(configuration);
        services.AddOperationsModule(configuration);
        services.AddAccessControlModule(configuration);
        services.AddWhatsAppModule(configuration);
        services.AddAIEngineModule(configuration);

        services.AddFastEndpoints(config =>
        {
            config.Assemblies =
            [
                typeof(Program).Assembly,
                typeof(Modules.Identity.Endpoints.LoginEndpoint).Assembly,
                typeof(Modules.Financial.Endpoints.GetInvoicesEndpoint).Assembly,
                typeof(Modules.Operations.Endpoints.CreateAreaComumEndpoint).Assembly,
                typeof(Modules.AccessControl.Infrastructure.AccessControlServiceCollectionExtensions).Assembly,
                typeof(Modules.WhatsApp.Endpoints.ReceiveEvolutionWebhookEndpoint).Assembly,
                typeof(Modules.AIEngine.Endpoints.GetAiConfigEndpoint).Assembly
            ];
        });

        return services;
    }

    /// <summary>
    /// Configura documentação OpenAPI
    /// </summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi("v1");

        return services;
    }
}
