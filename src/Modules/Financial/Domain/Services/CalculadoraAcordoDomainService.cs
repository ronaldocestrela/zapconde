using Modules.Financial.Domain.ValueObjects;

namespace Modules.Financial.Domain.Services;

/// <summary>
/// Domain Service responsável por simular e calcular os valores de um acordo de renegociação.
/// </summary>
public class CalculadoraAcordoDomainService
{
    public ResumoAcordoCalculado SimularAcordo(
        decimal valorTotalOriginal,
        decimal valorDescontoConcedido,
        int quantidadeParcelas,
        DateTime dataPrimeiroVencimento)
    {
        if (valorTotalOriginal <= 0)
            throw new ArgumentException("Valor total original deve ser positivo.", nameof(valorTotalOriginal));

        if (quantidadeParcelas <= 0)
            throw new ArgumentException("Quantidade de parcelas deve ser maior que zero.", nameof(quantidadeParcelas));

        var valorDescontoReal = Math.Min(valorDescontoConcedido, valorTotalOriginal);
        var valorTotalAcordo = valorTotalOriginal - valorDescontoReal;

        // Divisão com ajuste exato de centavos na última parcela
        var valorParcelaBase = Math.Floor((valorTotalAcordo / quantidadeParcelas) * 100m) / 100m;
        var somaParcelasBase = valorParcelaBase * quantidadeParcelas;
        var diferencaCentavos = valorTotalAcordo - somaParcelasBase;

        var parcelas = new List<ProjecaoParcelaAcordo>();
        var utcPrimeiroVencimento = dataPrimeiroVencimento.Kind == DateTimeKind.Utc
            ? dataPrimeiroVencimento
            : DateTime.SpecifyKind(dataPrimeiroVencimento, DateTimeKind.Utc);

        for (int i = 1; i <= quantidadeParcelas; i++)
        {
            var valorParcela = valorParcelaBase;
            if (i == quantidadeParcelas)
            {
                valorParcela += diferencaCentavos;
            }

            var vencimento = utcPrimeiroVencimento.AddMonths(i - 1);
            parcelas.Add(new ProjecaoParcelaAcordo(i, vencimento, valorParcela));
        }

        return new ResumoAcordoCalculado(
            ValorTotalOriginal: valorTotalOriginal,
            ValorDesconto: valorDescontoReal,
            ValorTotalAcordo: valorTotalAcordo,
            QuantidadeParcelas: quantidadeParcelas,
            ValorParcelaBase: valorParcelaBase,
            Parcelas: parcelas.AsReadOnly()
        );
    }
}
