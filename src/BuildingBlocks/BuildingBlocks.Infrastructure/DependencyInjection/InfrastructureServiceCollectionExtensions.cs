using BuildingBlocks.Infrastructure.Messaging;
using BuildingBlocks.Infrastructure.MultiTenancy;
using BuildingBlocks.Shared.MultiTenancy;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

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
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ");
        var rabbitMqSection = configuration.GetSection(RabbitMqOptions.SectionName);
        var rabbitMqOptions = new RabbitMqOptions
        {
            Host = rabbitMqSection["Host"] ?? string.Empty,
            Port = int.TryParse(rabbitMqSection["Port"], out var port) ? port : 0,
            VirtualHost = rabbitMqSection["VirtualHost"] ?? "/",
            Username = rabbitMqSection["Username"] ?? string.Empty,
            Password = rabbitMqSection["Password"] ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(postgresConnectionString))
        {
            throw new InvalidOperationException("Connection string 'ConnectionStrings:Postgres' não foi configurada.");
        }

        if (string.IsNullOrWhiteSpace(rabbitMqConnectionString))
        {
            ValidateRabbitMqOptions(rabbitMqOptions);
        }

        services.AddSingleton(rabbitMqOptions);

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                var hostUri = BuildRabbitMqHostUri(rabbitMqOptions!, rabbitMqConnectionString);

                cfg.Host(hostUri, host =>
                {
                    host.Username(rabbitMqOptions.Username);
                    host.Password(rabbitMqOptions.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddHealthChecks()
            .AddCheck<RabbitMqBusHealthCheck>(
                name: "rabbitmq",
                tags: ["ready", "rabbitmq"]);

        // Registra serviço de contexto de tenant (scoped para suportar isolamento por requisição)
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();

        return services;
    }

    private static void ValidateRabbitMqOptions(RabbitMqOptions? options)
    {
        if (options is null)
        {
            throw new InvalidOperationException("Seção 'RabbitMQ' não foi configurada.");
        }

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            throw new InvalidOperationException("RabbitMQ:Host não foi configurado.");
        }

        if (options.Port <= 0)
        {
            throw new InvalidOperationException("RabbitMQ:Port deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(options.Username))
        {
            throw new InvalidOperationException("RabbitMQ:Username não foi configurado.");
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException("RabbitMQ:Password não foi configurado.");
        }
    }

    private static Uri BuildRabbitMqHostUri(RabbitMqOptions options, string? rabbitMqConnectionString)
    {
        if (!string.IsNullOrWhiteSpace(rabbitMqConnectionString))
        {
            return new Uri(rabbitMqConnectionString);
        }

        var path = options.VirtualHost == "/"
            ? "/"
            : $"/{Uri.EscapeDataString(options.VirtualHost.Trim('/'))}";

        var uri = $"rabbitmq://{options.Host}:{options.Port.ToString(CultureInfo.InvariantCulture)}{path}";
        return new Uri(uri);
    }

}
