using BuildingBlocks.Shared.MultiTenancy;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Entidade de associação entre Acordo e Faturas Originais consolidadas.
/// </summary>
public class AcordoFaturaVinculada : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int AcordoId { get; set; }
    public int FaturaId { get; set; }
    public decimal ValorFaturaOriginal { get; set; }

    protected AcordoFaturaVinculada() { }

    public static AcordoFaturaVinculada Create(int tenantId, int acordoId, int faturaId, decimal valorFaturaOriginal)
    {
        return new AcordoFaturaVinculada
        {
            TenantId = tenantId,
            AcordoId = acordoId,
            FaturaId = faturaId,
            ValorFaturaOriginal = valorFaturaOriginal
        };
    }
}
