using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Health check para validar prontidão da conexão com Redis.
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var pingResult = await db.PingAsync();

            return HealthCheckResult.Healthy($"Redis operacional (latency: {pingResult.TotalMilliseconds:F2}ms).");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao comunicar com a instância Redis.", ex);
        }
    }
}
