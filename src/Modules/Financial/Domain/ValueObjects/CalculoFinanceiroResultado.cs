namespace Modules.Financial.Domain.ValueObjects;

/// <summary>
/// Value Object imutável contendo o resultado do cálculo de encargos financeiros e trilha de auditoria.
/// </summary>
public record CalculoFinanceiroResultado
{
    public decimal ValorOriginal { get; init; }
    public DateTime DataVencimento { get; init; }
    public DateTime DataCalculo { get; init; }
    public int DiasAtraso { get; init; }
    public decimal ValorMulta { get; init; }
    public decimal ValorJuros { get; init; }
    public decimal ValorDesconto { get; init; }
    public decimal ValorTotalCalculado { get; init; }
    public string MemoriaCalculoTextual { get; init; } = string.Empty;

    public CalculoFinanceiroResultado(
        decimal valorOriginal,
        DateTime dataVencimento,
        DateTime dataCalculo,
        int diasAtraso,
        decimal valorMulta,
        decimal valorJuros,
        decimal valorDesconto,
        decimal valorTotalCalculado,
        string memoriaCalculoTextual)
    {
        ValorOriginal = valorOriginal;
        DataVencimento = dataVencimento;
        DataCalculo = dataCalculo;
        DiasAtraso = diasAtraso;
        ValorMulta = valorMulta;
        ValorJuros = valorJuros;
        ValorDesconto = valorDesconto;
        ValorTotalCalculado = valorTotalCalculado;
        MemoriaCalculoTextual = memoriaCalculoTextual;
    }
}
