using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Configuração por condomínio das etapas da régua de cobrança de inadimplência.
/// </summary>
public class EtapaReguaInadimplencia : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public int Ordem { get; set; }
    public int DiasAtrasoMinimo { get; set; }
    public int DiasAtrasoMaximo { get; set; }
    public string NomeEtapa { get; set; } = string.Empty;
    public CanalCobranca Canal { get; set; } = CanalCobranca.WhatsApp;
    public TipoAcaoCobranca TipoAcao { get; set; } = TipoAcaoCobranca.LembreteAmigavel;
    public string TemplateMensagem { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    protected EtapaReguaInadimplencia() { }

    public static EtapaReguaInadimplencia Create(
        int tenantId,
        int condoId,
        int ordem,
        int diasAtrasoMinimo,
        int diasAtrasoMaximo,
        string nomeEtapa,
        CanalCobranca canal,
        TipoAcaoCobranca tipoAcao,
        string templateMensagem)
    {
        if (diasAtrasoMinimo < 0)
            throw new ArgumentException("Dias de atraso mínimo não pode ser negativo.", nameof(diasAtrasoMinimo));

        if (string.IsNullOrWhiteSpace(nomeEtapa))
            throw new ArgumentException("Nome da etapa é obrigatório.", nameof(nomeEtapa));

        return new EtapaReguaInadimplencia
        {
            TenantId = tenantId,
            CondoId = condoId,
            Ordem = ordem,
            DiasAtrasoMinimo = diasAtrasoMinimo,
            DiasAtrasoMaximo = diasAtrasoMaximo,
            NomeEtapa = nomeEtapa.Trim(),
            Canal = canal,
            TipoAcao = tipoAcao,
            TemplateMensagem = templateMensagem ?? string.Empty,
            Ativo = true
        };
    }

    public void AtualizarConfiguracao(
        int ordem,
        int diasAtrasoMinimo,
        int diasAtrasoMaximo,
        string nomeEtapa,
        CanalCobranca canal,
        TipoAcaoCobranca tipoAcao,
        string templateMensagem,
        bool ativo)
    {
        Ordem = ordem;
        DiasAtrasoMinimo = diasAtrasoMinimo;
        DiasAtrasoMaximo = diasAtrasoMaximo;
        NomeEtapa = nomeEtapa;
        Canal = canal;
        TipoAcao = tipoAcao;
        TemplateMensagem = templateMensagem;
        Ativo = ativo;
    }
}
