using BuildingBlocks.Shared.MultiTenancy;

namespace Modules.Identity.Domain;

/// <summary>
/// Vínculo de um usuário a um tenant/condomínio com papel RBAC.
/// </summary>
public class UserCondoMembership : ITenantScoped
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int TenantId { get; set; }

    public int CondoId { get; set; }

    public string Role { get; set; } = string.Empty;

    public string DisplayLabel { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsTenantActive { get; set; } = true;

    public ApplicationUser? User { get; set; }
}
