using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Services;

public class PastaDigitalDomainService
{
    public PastaDigital ConsolidarBalanceteMensal(
        PastaDigital pasta,
        IEnumerable<Fatura> faturasPagasNoMes,
        IEnumerable<ItemBalancete> despesasRegistradas)
    {
        ArgumentNullException.ThrowIfNull(pasta);

        foreach (var fatura in faturasPagasNoMes)
        {
            var dataPago = fatura.DataPagamento ?? fatura.DataVencimento;
            pasta.AdicionarItemBalancete(
                TipoLancamentoBalancete.Receita,
                CategoriaPlanoContas.ReceitaOrdinaria,
                $"Recebimento Taxa Condominial - Fatura #{fatura.NumeroFatura}",
                fatura.TotalFinal,
                fatura.TotalFinal,
                dataPago,
                conciliado: true);
        }

        foreach (var despesa in despesasRegistradas)
        {
            pasta.AdicionarItemBalancete(
                TipoLancamentoBalancete.Despesa,
                despesa.Categoria,
                despesa.Descricao,
                despesa.ValorOrcado,
                despesa.ValorRealizado,
                despesa.DataLancamento,
                conciliado: despesa.Conciliado);
        }

        pasta.RecalcularSaldos();
        pasta.ResumoExecutivoIa = $"Balanço mensal do condomínio: Total de Receitas = R$ {pasta.TotalReceitas:N2}, Total de Despesas = R$ {pasta.TotalDespesas:N2}. Saldo do Mês = R$ {pasta.SaldoMes:N2}.";
        return pasta;
    }
}
