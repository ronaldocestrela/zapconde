using BuildingBlocks.Shared.MultiTenancy;

namespace BuildingBlocks.Infrastructure.MultiTenancy;

/// <summary>
/// Implementação scoped do contexto de tenant por requisição.
/// Deny-by-default: retorna null quando o tenant não foi resolvido pelo middleware.
/// </summary>
public class CurrentTenantService : ICurrentTenantService
{
    private int? _tenantId;
    private int? _condoId;

    public int? TenantId => _tenantId;

    public int? CondoId => _condoId;

    public bool IsResolved => TenantId.HasValue;

    public void SetTenantId(int tenantId) => _tenantId = tenantId;

    public void SetCondoId(int condoId) => _condoId = condoId;

    public void Clear()
    {
        _tenantId = null;
        _condoId = null;
    }
}
