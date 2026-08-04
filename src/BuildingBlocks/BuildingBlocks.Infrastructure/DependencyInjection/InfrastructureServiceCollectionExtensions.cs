using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Messaging;
using BuildingBlocks.Infrastructure.MultiTenancy;
using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.MultiTenancy;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
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

        var redisConnectionString = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(postgresConnectionString))
        {
            throw new InvalidOperationException("Connection string 'ConnectionStrings:Postgres' não foi configurada.");
        }

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("Connection string 'ConnectionStrings:Redis' não foi configurada.");
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

        // Configuração Redis / Cache
        var useInMemoryCache = configuration.GetValue<bool>("Infrastructure:UseInMemoryCache");
        if (useInMemoryCache)
        {
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }
        else
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();
            services.AddScoped<IChatSessionService, RedisChatSessionService>();

            services.AddHealthChecks()
                .AddCheck<RedisHealthCheck>(
                    name: "redis",
                    tags: ["ready", "redis"]);
        }

        services.AddHealthChecks()
            .AddCheck<RabbitMqBusHealthCheck>(
                name: "rabbitmq",
                tags: ["ready", "rabbitmq"]);

        // Registra serviço de contexto de tenant (scoped para suportar isolamento por requisição)
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();

        return services;
    }

    /// <summary>
    /// Habilita o Transactional Outbox Pattern com EF Core e PostgreSQL para o DbContext especificado no MassTransit.
    /// </summary>
    public static IBusRegistrationConfigurator AddMassTransitOutbox<TDbContext>(this IBusRegistrationConfigurator busConfigurator)
        where TDbContext : DbContext
    {
        busConfigurator.AddEntityFrameworkOutbox<TDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });

        return busConfigurator;
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
