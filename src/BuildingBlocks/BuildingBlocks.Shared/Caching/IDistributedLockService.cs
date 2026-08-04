namespace BuildingBlocks.Shared.Caching;

/// <summary>
/// Representation of an acquired distributed lock handle.
/// </summary>
public interface IDistributedLockHandle : IAsyncDisposable
{
    /// <summary>
    /// Key identifier of the lock.
    /// </summary>
    string ResourceKey { get; }

    /// <summary>
    /// Unique token/value bound to this acquired lock handle.
    /// </summary>
    string LockValue { get; }

    /// <summary>
    /// Indicates if the lock is currently held by this handle instance.
    /// </summary>
    bool IsAcquired { get; }
}

/// <summary>
/// Contrato de serviço para gerenciar distributed locks distribuídos via Redis.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Tenta adquirir um lock distribuído assincronamente.
    /// Retorna um handle descartável com IsAcquired = true em caso de sucesso, ou IsAcquired = false em caso de falha/timeout.
    /// </summary>
    Task<IDistributedLockHandle> AcquireLockAsync(
        string resourceKey,
        TimeSpan expiry,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default);
}
