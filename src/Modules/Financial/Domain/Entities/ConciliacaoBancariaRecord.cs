using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Registro auditável da conciliação bancária entre item de extrato e origem interna do sistema.
/// </summary>
public class ConciliacaoBancariaRecord : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ExtratoBancarioItemId { get; set; }
    public OrigemConciliacao OrigemTipo { get; set; }
    public int OrigemId { get; set; }
    public DateTime DataConciliacao { get; set; } = DateTime.UtcNow;
    public int? ConciliadoPorUserId { get; set; }
    public string Observacoes { get; set; } = string.Empty;

    protected ConciliacaoBancariaRecord() { }

    public static ConciliacaoBancariaRecord Create(
        int tenantId,
        int extratoBancarioItemId,
        OrigemConciliacao origemTipo,
        int origemId,
        int? conciliadoPorUserId = null,
        string observacoes = "")
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId inválido.", nameof(tenantId));
        if (extratoBancarioItemId <= 0) throw new ArgumentException("ExtratoBancarioItemId inválido.", nameof(extratoBancarioItemId));
        if (origemId <= 0) throw new ArgumentException("OrigemId inválido.", nameof(origemId));

        return new ConciliacaoBancariaRecord
        {
            TenantId = tenantId,
            ExtratoBancarioItemId = extratoBancarioItemId,
            OrigemTipo = origemTipo,
            OrigemId = origemId,
            DataConciliacao = DateTime.UtcNow,
            ConciliadoPorUserId = conciliadoPorUserId,
            Observacoes = observacoes ?? string.Empty
        };
    }
}
