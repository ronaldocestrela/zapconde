using BuildingBlocks.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Financial.Application.Services;
using Modules.Financial.Infrastructure.Persistence;
using Modules.Financial.Infrastructure.Services;

namespace Modules.Financial.Infrastructure;

public static class FinancialServiceCollectionExtensions
{
    public static IServiceCollection AddFinancialModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FinancialDbContext>((sp, options) =>
        {
            var useInMemory = configuration.GetValue<bool>("Financial:UseInMemoryDatabase");
            var connectionString = configuration.GetConnectionString("Postgres");

            if (useInMemory || string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseInMemoryDatabase(configuration["Financial:InMemoryDatabaseName"] ?? "SmartCondoFinancial");
                return;
            }

            options.UseNpgsql(connectionString);
        });

        services.AddSingleton<Domain.Services.CalculadoraFinanceira>();
        services.AddSingleton<Domain.Services.CalculadoraAcordoDomainService>();
        services.AddSingleton<Domain.Services.ReguaInadimplenciaEngine>();
        services.AddSingleton<Domain.Services.PastaDigitalDomainService>();
        services.AddSingleton<Domain.Services.ConciliacaoBancariaDomainService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IFinancialCalculationService, FinancialCalculationService>();
        services.AddScoped<IAcordoApplicationService, AcordoApplicationService>();
        services.AddScoped<IReguaInadimplenciaAppService, ReguaInadimplenciaAppService>();
        services.AddScoped<IPastaDigitalApplicationService, PastaDigitalApplicationService>();
        services.AddScoped<IConciliacaoBancariaApplicationService, ConciliacaoBancariaApplicationService>();
        services.AddScoped<IRelatorioConsolidadoApplicationService, RelatorioConsolidadoApplicationService>();

        // Gateway de Pagamento, Stubs e Webhooks
        services.AddSingleton<MockPaymentGatewayService>();
        services.AddHttpClient<IPaymentGatewayService, AsaasPaymentGatewayService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
        services.AddScoped<IInvoicePaymentApplicationService, InvoicePaymentApplicationService>();

        return services;
    }
}

