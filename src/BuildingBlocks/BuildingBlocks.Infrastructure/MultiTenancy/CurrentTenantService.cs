using BuildingBlocks.Shared.MultiTenancy;

namespace BuildingBlocks.Infrastructure.MultiTenancy;

/// <summary>
/// Implementação padrão do serviço de contexto de tenant.
/// Retorna null quando o tenant não foi resolvido, garantindo segurança por padrão (deny-by-default).
/// A resolução do tenant será implementada em fase futura via Middleware que extrai do JWT ou Header.
/// </summary>
public class CurrentTenantService : ICurrentTenantService
{
    private int? _tenantId;

    /// <summary>
    /// ID do tenant atual. Retorna null quando não resolvido (estado seguro padrão).
    /// </summary>
    public int? TenantId => _tenantId;

    /// <summary>
    /// Define o tenant atual. Usado pelo Middleware de autenticação (fase futura).
    /// </summary>
    /// <param name="tenantId">ID do tenant a ser definido no contexto atual</param>
    public void SetTenantId(int tenantId)
    {
        _tenantId = tenantId;
    }

    /// <summary>
    /// Limpa o tenant atual do contexto.
    /// </summary>
    public void ClearTenantId()
    {
        _tenantId = null;
    }
}
