namespace BuildingBlocks.Shared.MultiTenancy;

/// <summary>
/// Nomes canônicos de headers HTTP para resolução de contexto multi-tenant em webhooks.
/// </summary>
public static class TenantHttpHeaders
{
    public const string TenantId = "X-Tenant-Id";
    public const string CondoId = "X-Condo-Id";
}
