namespace Modules.Identity.Domain;

/// <summary>
/// Nomes canônicos das claims JWT obrigatórias do SmartCondo.
/// </summary>
public static class SmartCondoClaimTypes
{
    public const string TenantId = "TenantId";
    public const string CondoId = "CondoId";
    public const string UserId = "UserId";
    public const string Role = "Role";
    public const string MembershipId = "MembershipId";
}
