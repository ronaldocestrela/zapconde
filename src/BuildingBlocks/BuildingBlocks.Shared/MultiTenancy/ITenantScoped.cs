namespace BuildingBlocks.Shared.MultiTenancy;

/// <summary>
/// Marca uma entidade como escopo de tenant, aplicando isolamento automático por tenant_id.
/// Entidades que implementam esta interface são automaticamente filtradas pelo Global Query Filter do EF Core.
/// </summary>
public interface ITenantScoped
{
    /// <summary>
    /// Identificador único do tenant (condomínio/administradora) ao qual a entidade pertence.
    /// Utilizado para isolamento de dados em arquitetura multi-tenant.
    /// </summary>
    int TenantId { get; set; }
}
