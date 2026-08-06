using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Registro auditável de disparos de cobrança pela régua de inadimplência.
/// </summary>
public class HistoricoCobranca : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public int UnidadeId { get; set; }
    public int MoradorId { get; set; }
    public int FaturaId { get; set; }
    public int EtapaReguaId { get; set; }

    public DateTime DataExecucao { get; set; } = DateTime.UtcNow;
    public CanalCobranca Canal { get; set; }
    public TipoAcaoCobranca TipoAcao { get; set; }
    public string MensagemEnviada { get; set; } = string.Empty;
    public bool Sucesso { get; set; } = true;
    public string Observacao { get; set; } = string.Empty;

    protected HistoricoCobranca() { }

    public static HistoricoCobranca Create(
        int tenantId,
        int condoId,
        int unidadeId,
        int moradorId,
        int faturaId,
        int etapaReguaId,
        CanalCobranca canal,
        TipoAcaoCobranca tipoAcao,
        string mensagemEnviada,
        bool sucesso = true,
        string observacao = "")
    {
        return new HistoricoCobranca
        {
            TenantId = tenantId,
            CondoId = condoId,
            UnidadeId = unidadeId,
            MoradorId = moradorId,
            FaturaId = faturaId,
            EtapaReguaId = etapaReguaId,
            DataExecucao = DateTime.UtcNow,
            Canal = canal,
            TipoAcao = tipoAcao,
            MensagemEnviada = mensagemEnviada ?? string.Empty,
            Sucesso = sucesso,
            Observacao = observacao ?? string.Empty
        };
    }
}
