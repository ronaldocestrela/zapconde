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
            if (useInMemory)
            {
                options.UseInMemoryDatabase(configuration["Financial:InMemoryDatabaseName"] ?? "SmartCondoFinancial");
                return;
            }

            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' não configurada.");

            options.UseNpgsql(connectionString);
        });

        services.AddSingleton<Domain.Services.CalculadoraFinanceira>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IFinancialCalculationService, FinancialCalculationService>();

        // Gateway de Pagamento, Stubs e Webhooks
        services.AddSingleton<MockPaymentGatewayService>();
        services.AddHttpClient<IPaymentGatewayService, AsaasPaymentGatewayService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
        services.AddScoped<IInvoicePaymentApplicationService, InvoicePaymentApplicationService>();

        return services;
    }
}

