using Microsoft.AspNetCore.Identity;

namespace Modules.Identity.Domain;

/// <summary>
/// Usuário de autenticação global (sem tenant_id no registro base).
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<UserCondoMembership> Memberships { get; set; } = [];
}
