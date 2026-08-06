using System.Collections.Concurrent;
using BuildingBlocks.Shared.Caching;

namespace BuildingBlocks.Infrastructure.Caching;

public sealed class InMemoryDistributedLockHandle : IDistributedLockHandle
{
    private readonly SemaphoreSlim _semaphore;
    private int _isDisposed;

    public string ResourceKey { get; }
    public string LockValue { get; }
    public bool IsAcquired { get; private set; }

    public InMemoryDistributedLockHandle(SemaphoreSlim semaphore, string resourceKey, string lockValue, bool isAcquired)
    {
        _semaphore = semaphore;
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
                _semaphore.Release();
            }
            finally
            {
                IsAcquired = false;
            }
        }

        await Task.CompletedTask;
    }
}

public class InMemoryDistributedLockService : IDistributedLockService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public async Task<IDistributedLockHandle> AcquireLockAsync(
        string resourceKey,
        TimeSpan expiry,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        var semaphore = Locks.GetOrAdd(resourceKey, _ => new SemaphoreSlim(1, 1));
        var lockValue = Guid.NewGuid().ToString();

        var timeoutMs = timeout > TimeSpan.Zero ? (int)timeout.TotalMilliseconds : 0;
        var acquired = await semaphore.WaitAsync(timeoutMs, cancellationToken);

        return new InMemoryDistributedLockHandle(semaphore, resourceKey, lockValue, isAcquired: acquired);
    }
}
