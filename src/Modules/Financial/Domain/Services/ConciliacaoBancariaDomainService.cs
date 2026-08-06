using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Services;

public class MatchConciliacaoResultado
{
    public ExtratoBancarioItem ExtratoItem { get; set; } = null!;
    public OrigemConciliacao OrigemTipo { get; set; }
    public int OrigemId { get; set; }
    public decimal ScoreMatch { get; set; }
}

public class ConciliacaoBancariaDomainService
{
    public IEnumerable<MatchConciliacaoResultado> ProcessarConciliacaoAutomatica(
        IEnumerable<ExtratoBancarioItem> itensExtrato,
        IEnumerable<Fatura> faturasLiquidadas,
        IEnumerable<ItemBalancete> despesas)
    {
        var resultados = new List<MatchConciliacaoResultado>();

        foreach (var item in itensExtrato.Where(i => i.StatusConciliacao == StatusConciliacaoBancaria.Pendente))
        {
            if (item.TipoTransacao == TipoTransacaoBancaria.Credito)
            {
                // Tenta match com Fatura Liquidada
                var faturaMatch = faturasLiquidadas.FirstOrDefault(f =>
                    Math.Abs(f.TotalFinal - item.Valor) < 0.01m &&
                    (f.DataPagamento?.Date == item.DataTransacao.Date || Math.Abs((f.DataPagamento?.Date - item.DataTransacao.Date)?.TotalDays ?? 99) <= 2));

                if (faturaMatch != null)
                {
                    decimal score = 100m;
                    if (faturaMatch.DataPagamento?.Date != item.DataTransacao.Date)
                        score = 85m;

                    item.ConciliarAutomatico(faturaMatch.Id, score);
                    resultados.Add(new MatchConciliacaoResultado
                    {
                        ExtratoItem = item,
                        OrigemTipo = OrigemConciliacao.Fatura,
                        OrigemId = faturaMatch.Id,
                        ScoreMatch = score
                    });
                }
            }
            else if (item.TipoTransacao == TipoTransacaoBancaria.Debito)
            {
                // Tenta match com Despesa
                var despesaMatch = despesas.FirstOrDefault(d =>
                    Math.Abs(d.ValorRealizado - item.Valor) < 0.01m &&
                    Math.Abs((d.DataLancamento.Date - item.DataTransacao.Date).TotalDays) <= 2);

                if (despesaMatch != null)
                {
                    decimal score = 100m;
                    if (despesaMatch.DataLancamento.Date != item.DataTransacao.Date)
                        score = 85m;

                    item.ConciliarAutomatico(despesaMatch.Id, score);
                    despesaMatch.Conciliado = true;
                    resultados.Add(new MatchConciliacaoResultado
                    {
                        ExtratoItem = item,
                        OrigemTipo = OrigemConciliacao.DespesaBalancete,
                        OrigemId = despesaMatch.Id,
                        ScoreMatch = score
                    });
                }
            }
        }

        return resultados;
    }
}
