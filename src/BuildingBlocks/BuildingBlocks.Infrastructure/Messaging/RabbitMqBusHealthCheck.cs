using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Health check do broker RabbitMQ baseado no estado de saúde do bus do MassTransit.
/// </summary>
public sealed class RabbitMqBusHealthCheck : IHealthCheck
{
    private readonly IBusControl _busControl;

    public RabbitMqBusHealthCheck(IBusControl busControl)
    {
        _busControl = busControl;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var busHealth = _busControl.CheckHealth();

        return Task.FromResult(busHealth.Status switch
        {
            BusHealthStatus.Healthy => HealthCheckResult.Healthy("RabbitMQ está saudável."),
            BusHealthStatus.Degraded => HealthCheckResult.Degraded("RabbitMQ está degradado."),
            _ => HealthCheckResult.Unhealthy("RabbitMQ está indisponível.")
        });
    }
}
