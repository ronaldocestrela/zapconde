namespace BuildingBlocks.Shared.Caching;

/// <summary>
/// Contrato de serviço para operações de cache com suporte a isolamento por tenant.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Recupera um objeto do cache pelo nome da chave.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Armazena um objeto no cache com expiração opcional.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma chave do cache.
    /// </summary>
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se uma chave existe no cache.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
