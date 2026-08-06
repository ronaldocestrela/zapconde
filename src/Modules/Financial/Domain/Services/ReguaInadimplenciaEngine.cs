using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Domain.Services;

/// <summary>
/// Domain Service que cruza faturas vencidas em atraso com a régua configurada do condomínio.
/// </summary>
public class ReguaInadimplenciaEngine
{
    public IEnumerable<(Fatura Fatura, EtapaReguaInadimplencia Etapa)> AvaliarFaturasElegiveis(
        IEnumerable<Fatura> faturasVencidas,
        IEnumerable<EtapaReguaInadimplencia> etapasRegua,
        IEnumerable<HistoricoCobranca> historicosExistentes,
        DateTime dataReferencia)
    {
        var acoesElegiveis = new List<(Fatura Fatura, EtapaReguaInadimplencia Etapa)>();
        var etapasAtivas = etapasRegua.Where(e => e.Ativo).OrderBy(e => e.Ordem).ToList();

        if (!etapasAtivas.Any())
            return acoesElegiveis;

        var utcHoje = dataReferencia.Kind == DateTimeKind.Utc
            ? dataReferencia
            : DateTime.SpecifyKind(dataReferencia, DateTimeKind.Utc);

        foreach (var fatura in faturasVencidas)
        {
            if (fatura.Status != Enums.StatusFatura.Vencido && fatura.DataVencimento >= utcHoje)
                continue;

            var diasAtraso = (int)(utcHoje.Date - fatura.DataVencimento.Date).TotalDays;
            if (diasAtraso <= 0)
                continue;

            // Encontra a etapa correspondente ao intervalo de atraso
            var etapaElegivel = etapasAtivas.FirstOrDefault(e =>
                diasAtraso >= e.DiasAtrasoMinimo &&
                (e.DiasAtrasoMaximo <= 0 || diasAtraso <= e.DiasAtrasoMaximo));

            if (etapaElegivel == null)
                continue;

            // Verifica se essa etapa já foi executada para essa fatura
            var jaExecutado = historicosExistentes.Any(h =>
                h.FaturaId == fatura.Id &&
                h.EtapaReguaId == etapaElegivel.Id &&
                h.Sucesso);

            if (!jaExecutado)
            {
                acoesElegiveis.Add((fatura, etapaElegivel));
            }
        }

        return acoesElegiveis;
    }
}
