using System.Globalization;
using System.Text;
using Modules.Financial.Domain.ValueObjects;

namespace Modules.Financial.Domain.Services;

/// <summary>
/// Domain Service responsável pela execução matemática e determinística do cálculo de encargos e descontos.
/// </summary>
public class CalculadoraFinanceira
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public CalculoFinanceiroResultado CalcularEncargos(ParametrosCalculoFinanceiro parametros)
    {
        ArgumentNullException.ThrowIfNull(parametros);

        var dataVenc = parametros.DataVencimento.Date;
        var dataCalc = parametros.DataCalculo.Date;

        var diasAtraso = dataCalc > dataVenc ? (int)(dataCalc - dataVenc).TotalDays : 0;
        decimal valorMulta = 0m;
        decimal valorJuros = 0m;
        decimal valorDesconto = 0m;
        var sbAudit = new StringBuilder();

        sbAudit.AppendLine(string.Format(PtBr, "Valor Original: R$ {0:N2}", parametros.ValorOriginal));
        sbAudit.AppendLine(string.Format(PtBr, "Data de Vencimento: {0:dd/MM/yyyy}", dataVenc));
        sbAudit.AppendLine(string.Format(PtBr, "Data de Referência/Pagamento: {0:dd/MM/yyyy}", dataCalc));

        if (diasAtraso == 0)
        {
            sbAudit.AppendLine("Status: Em dia / Dentro do prazo de vencimento.");

            var limiteDesconto = parametros.DataLimiteDesconto ?? dataVenc;
            if (dataCalc <= limiteDesconto)
            {
                if (parametros.ValorDescontoPontualidade > 0)
                {
                    valorDesconto += parametros.ValorDescontoPontualidade;
                }

                if (parametros.PercentualDescontoPontualidade > 0)
                {
                    valorDesconto += Math.Round(parametros.ValorOriginal * (parametros.PercentualDescontoPontualidade / 100m), 2, MidpointRounding.AwayFromZero);
                }

                if (valorDesconto > parametros.ValorOriginal)
                {
                    valorDesconto = parametros.ValorOriginal;
                }

                if (valorDesconto > 0)
                {
                    sbAudit.AppendLine(string.Format(PtBr, "Desconto de Pontualidade Aplicado: -R$ {0:N2}", valorDesconto));
                }
            }
        }
        else
        {
            sbAudit.AppendLine(string.Format(PtBr, "Status: Em Atraso ({0} dia(s) corrido(s)).", diasAtraso));

            // Multa por Atraso (uma única vez)
            if (parametros.PercentualMulta > 0)
            {
                valorMulta = Math.Round(parametros.ValorOriginal * (parametros.PercentualMulta / 100m), 2, MidpointRounding.AwayFromZero);
                sbAudit.AppendLine(string.Format(PtBr, "Multa por Atraso ({0:N1}%): R$ {1:N2}", parametros.PercentualMulta, valorMulta));
            }

            // Juros de Mora Pró-Rata Dia (1% a.m. = 0.033333% a.d.)
            if (parametros.PercentualJurosMensal > 0)
            {
                var taxaDiaria = (parametros.PercentualJurosMensal / 30m) / 100m;
                valorJuros = Math.Round(parametros.ValorOriginal * taxaDiaria * diasAtraso, 2, MidpointRounding.AwayFromZero);
                var taxaDiariaPct = (parametros.PercentualJurosMensal / 30m);
                sbAudit.AppendLine(string.Format(PtBr, "Juros Pró-Rata ({0:N1}% a.m. = {1:F4}% a.d. × {2} dias): R$ {3:N2}", parametros.PercentualJurosMensal, taxaDiariaPct, diasAtraso, valorJuros));
            }

            sbAudit.AppendLine("Desconto de Pontualidade: R$ 0,00 (expirado por atraso).");
        }

        var valorTotalCalculado = Math.Round(parametros.ValorOriginal + valorMulta + valorJuros - valorDesconto, 2, MidpointRounding.AwayFromZero);
        sbAudit.AppendLine(string.Format(PtBr, "TOTAL FINAL A PAGAR: R$ {0:N2}", valorTotalCalculado));

        return new CalculoFinanceiroResultado(
            valorOriginal: parametros.ValorOriginal,
            dataVencimento: dataVenc,
            dataCalculo: dataCalc,
            diasAtraso: diasAtraso,
            valorMulta: valorMulta,
            valorJuros: valorJuros,
            valorDesconto: valorDesconto,
            valorTotalCalculado: valorTotalCalculado,
            memoriaCalculoTextual: sbAudit.ToString().TrimEnd()
        );
    }
}
