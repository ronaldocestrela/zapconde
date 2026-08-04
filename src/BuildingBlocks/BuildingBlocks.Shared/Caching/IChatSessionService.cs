namespace BuildingBlocks.Shared.Caching;

/// <summary>
/// Contrato de serviço para gerenciar o estado da sessão de chat (WhatsApp/IA) no Redis com expiração configurável.
/// </summary>
public interface IChatSessionService
{
    /// <summary>
    /// Recupera o estado da sessão de chat pelo identificador da conversa ou telefone.
    /// </summary>
    Task<T?> GetSessionStateAsync<T>(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Salva ou atualiza o estado da sessão de chat com TTL configurável.
    /// </summary>
    Task SetSessionStateAsync<T>(string sessionId, T sessionData, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpa o estado da sessão de chat.
    /// </summary>
    Task<bool> ClearSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}
