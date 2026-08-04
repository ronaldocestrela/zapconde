using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.MultiTenancy;
using StackExchange.Redis;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Implementação de ICacheService baseada em Redis com isolamento automático de tenant.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ICurrentTenantService _tenantService;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public RedisCacheService(IConnectionMultiplexer redis, ICurrentTenantService tenantService)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var tenantKey = BuildTenantKey(key);
        var redisValue = await db.StringGetAsync(tenantKey);

        if (!redisValue.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(redisValue.ToString(), JsonSerializerOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var tenantKey = BuildTenantKey(key);
        var jsonValue = JsonSerializer.Serialize(value, JsonSerializerOptions);

        await db.StringSetAsync(tenantKey, jsonValue, expiration);
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var tenantKey = BuildTenantKey(key);
        return await db.KeyDeleteAsync(tenantKey);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var tenantKey = BuildTenantKey(key);
        return await db.KeyExistsAsync(tenantKey);
    }

    private string BuildTenantKey(string key)
    {
        var tenantId = _tenantService.TenantId;
        return tenantId.HasValue
            ? $"tenant:{tenantId.Value}:{key}"
            : $"global:{key}";
    }
}
