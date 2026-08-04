namespace BuildingBlocks.Shared.MultiTenancy;

/// <summary>
/// Serviço responsável por fornecer o contexto do tenant atual na requisição.
/// Implementado pela infraestrutura e injetado no DbContext para aplicar filtros de multi-tenancy.
/// </summary>
public interface ICurrentTenantService
{
    /// <summary>
    /// ID do tenant atual extraído do token JWT, header de Webhook ou contexto de execução.
    /// Retorna null quando o tenant não foi resolvido (ex.: requisição não autenticada).
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// ID do condomínio atual extraído do token JWT ou header de Webhook.
    /// </summary>
    int? CondoId { get; }

    /// <summary>
    /// Indica se há um tenant resolvido no contexto atual.
    /// </summary>
    bool IsResolved => TenantId.HasValue;

    /// <summary>
    /// Define o tenant atual no contexto da requisição.
    /// </summary>
    void SetTenantId(int tenantId);

    /// <summary>
    /// Define o condomínio atual no contexto da requisição.
    /// </summary>
    void SetCondoId(int condoId);

    /// <summary>
    /// Limpa tenant e condomínio do contexto atual.
    /// </summary>
    void Clear();
}
