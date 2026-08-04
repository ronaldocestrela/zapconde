using BuildingBlocks.Shared.Caching;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Handle de lock distribuído descartável.
/// </summary>
public sealed class RedisDistributedLockHandle : IDistributedLockHandle
{
    private readonly IDatabase _database;
    private int _isDisposed;

    public string ResourceKey { get; }
    public string LockValue { get; }
    public bool IsAcquired { get; private set; }

    public RedisDistributedLockHandle(IDatabase database, string resourceKey, string lockValue, bool isAcquired)
    {
        _database = database;
        ResourceKey = resourceKey;
        LockValue = lockValue;
        IsAcquired = isAcquired;
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
        {
            return;
        }

        if (IsAcquired)
        {
            try
            {
                await _database.LockReleaseAsync(ResourceKey, LockValue);
            }
            finally
            {
                IsAcquired = false;
            }
        }
    }
}

/// <summary>
/// Implementação de IDistributedLockService baseada em Redis LockTake/LockRelease.
/// </summary>
public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedLockService(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    public async Task<IDistributedLockHandle> AcquireLockAsync(
        string resourceKey,
        TimeSpan expiry,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var lockValue = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        do
        {
            var acquired = await db.LockTakeAsync(resourceKey, lockValue, expiry);
            if (acquired)
            {
                return new RedisDistributedLockHandle(db, resourceKey, lockValue, isAcquired: true);
            }

            if (timeout <= TimeSpan.Zero)
            {
                break;
            }

            await Task.Delay(50, cancellationToken);
        } while (DateTime.UtcNow - startTime < timeout && !cancellationToken.IsCancellationRequested);

        return new RedisDistributedLockHandle(db, resourceKey, lockValue, isAcquired: false);
    }
}
