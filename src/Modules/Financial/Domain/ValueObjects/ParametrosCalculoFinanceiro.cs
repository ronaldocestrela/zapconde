namespace Modules.Financial.Domain.ValueObjects;

/// <summary>
/// Value Object imutável contendo os parâmetros de entrada para o cálculo ou simulação de encargos financeiros.
/// </summary>
public record ParametrosCalculoFinanceiro
{
    public decimal ValorOriginal { get; init; }
    public DateTime DataVencimento { get; init; }
    public DateTime DataCalculo { get; init; }
    public decimal PercentualMulta { get; init; } = 2.0m; // CC Art. 1336 § 1º
    public decimal PercentualJurosMensal { get; init; } = 1.0m; // 1% ao mês pró-rata dia
    public decimal ValorDescontoPontualidade { get; init; }
    public decimal PercentualDescontoPontualidade { get; init; }
    public DateTime? DataLimiteDesconto { get; init; }

    public ParametrosCalculoFinanceiro(
        decimal valorOriginal,
        DateTime dataVencimento,
        DateTime dataCalculo,
        decimal percentualMulta = 2.0m,
        decimal percentualJurosMensal = 1.0m,
        decimal valorDescontoPontualidade = 0m,
        decimal percentualDescontoPontualidade = 0m,
        DateTime? dataLimiteDesconto = null)
    {
        if (valorOriginal <= 0)
            throw new ArgumentOutOfRangeException(nameof(valorOriginal), "Valor original deve ser maior que zero.");

        if (percentualMulta < 0)
            throw new ArgumentOutOfRangeException(nameof(percentualMulta), "Percentual de multa não pode ser negativo.");

        if (percentualJurosMensal < 0)
            throw new ArgumentOutOfRangeException(nameof(percentualJurosMensal), "Percentual de juros mensal não pode ser negativo.");

        ValorOriginal = valorOriginal;
        DataVencimento = dataVencimento.Kind == DateTimeKind.Utc ? dataVencimento.Date : DateTime.SpecifyKind(dataVencimento.Date, DateTimeKind.Utc);
        DataCalculo = dataCalculo.Kind == DateTimeKind.Utc ? dataCalculo.Date : DateTime.SpecifyKind(dataCalculo.Date, DateTimeKind.Utc);
        PercentualMulta = percentualMulta;
        PercentualJurosMensal = percentualJurosMensal;
        ValorDescontoPontualidade = valorDescontoPontualidade;
        PercentualDescontoPontualidade = percentualDescontoPontualidade;
        DataLimiteDesconto = dataLimiteDesconto.HasValue
            ? (dataLimiteDesconto.Value.Kind == DateTimeKind.Utc ? dataLimiteDesconto.Value.Date : DateTime.SpecifyKind(dataLimiteDesconto.Value.Date, DateTimeKind.Utc))
            : null;
    }
}
