using BuildingBlocks.Shared.Caching;

namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Implementação de IChatSessionService para gestão de sessões efêmeras de chat via Redis.
/// </summary>
public class RedisChatSessionService : IChatSessionService
{
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(30);

    public RedisChatSessionService(ICacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<T?> GetSessionStateAsync<T>(string sessionId, CancellationToken cancellationToken = default)
    {
        var key = FormatSessionKey(sessionId);
        return await _cacheService.GetAsync<T>(key, cancellationToken);
    }

    public async Task SetSessionStateAsync<T>(string sessionId, T sessionData, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var key = FormatSessionKey(sessionId);
        var effectiveExpiry = expiry ?? DefaultExpiry;
        await _cacheService.SetAsync(key, sessionData, effectiveExpiry, cancellationToken);
    }

    public async Task<bool> ClearSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var key = FormatSessionKey(sessionId);
        return await _cacheService.RemoveAsync(key, cancellationToken);
    }

    private static string FormatSessionKey(string sessionId)
    {
        return $"chatsession:{sessionId}";
    }
}
