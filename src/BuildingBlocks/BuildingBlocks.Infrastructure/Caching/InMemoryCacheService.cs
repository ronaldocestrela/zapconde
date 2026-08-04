using System.Collections.Concurrent;
using System.Text.Json;
using BuildingBlocks.Shared.Caching;

namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Implementação in-memory de ICacheService para ambientes de teste sem Redis.
/// </summary>
public sealed class InMemoryCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<string, CacheEntry> _store = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(key, out var entry) || entry.IsExpired)
        {
            _store.TryRemove(key, out _);
            return Task.FromResult<T?>(default);
        }

        return Task.FromResult(JsonSerializer.Deserialize<T>(entry.Payload, JsonOptions));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(value, JsonOptions);
        var expiresAt = expiration.HasValue ? DateTimeOffset.UtcNow.Add(expiration.Value) : (DateTimeOffset?)null;
        _store[key] = new CacheEntry(payload, expiresAt);
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_store.TryRemove(key, out _));
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(key, out var entry) || entry.IsExpired)
        {
            _store.TryRemove(key, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    private sealed record CacheEntry(string Payload, DateTimeOffset? ExpiresAt)
    {
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTimeOffset.UtcNow;
    }
}
