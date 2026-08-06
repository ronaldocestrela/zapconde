using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Lançamento do extrato bancário importado para conciliação.
/// </summary>
public class ExtratoBancarioItem : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ContaBancariaId { get; set; }
    public DateTime DataTransacao { get; set; }
    public string DescricaoHistorico { get; set; } = string.Empty;
    public string DocumentoRef { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacaoBancaria TipoTransacao { get; set; }
    public StatusConciliacaoBancaria StatusConciliacao { get; set; } = StatusConciliacaoBancaria.Pendente;
    public int? TransacaoConciliadaId { get; set; }
    public decimal ScoreConciliacao { get; set; }

    protected ExtratoBancarioItem() { }

    public static ExtratoBancarioItem Create(
        int tenantId,
        int contaBancariaId,
        DateTime dataTransacao,
        string descricaoHistorico,
        string documentoRef,
        decimal valor,
        TipoTransacaoBancaria tipoTransacao)
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId inválido.", nameof(tenantId));
        if (contaBancariaId <= 0) throw new ArgumentException("ContaBancariaId inválido.", nameof(contaBancariaId));
        if (valor <= 0) throw new ArgumentException("Valor da transação deve ser positivo.", nameof(valor));

        var utcDataTransacao = dataTransacao.Kind == DateTimeKind.Utc
            ? dataTransacao
            : DateTime.SpecifyKind(dataTransacao, DateTimeKind.Utc);

        return new ExtratoBancarioItem
        {
            TenantId = tenantId,
            ContaBancariaId = contaBancariaId,
            DataTransacao = utcDataTransacao,
            DescricaoHistorico = descricaoHistorico ?? string.Empty,
            DocumentoRef = documentoRef ?? string.Empty,
            Valor = valor,
            TipoTransacao = tipoTransacao,
            StatusConciliacao = StatusConciliacaoBancaria.Pendente,
            ScoreConciliacao = 0
        };
    }

    public void ConciliarAutomatico(int transacaoOrigemId, decimal scoreMatch)
    {
        TransacaoConciliadaId = transacaoOrigemId;
        ScoreConciliacao = scoreMatch;
        StatusConciliacao = StatusConciliacaoBancaria.ConciliadoAutomatico;
    }

    public void ConciliarManual(int transacaoOrigemId)
    {
        TransacaoConciliadaId = transacaoOrigemId;
        ScoreConciliacao = 100;
        StatusConciliacao = StatusConciliacaoBancaria.ConciliadoManual;
    }

    public void MarcarDivergente()
    {
        StatusConciliacao = StatusConciliacaoBancaria.Divergencia;
    }

    public void Ignorar()
    {
        StatusConciliacao = StatusConciliacaoBancaria.Ignorado;
    }
}
