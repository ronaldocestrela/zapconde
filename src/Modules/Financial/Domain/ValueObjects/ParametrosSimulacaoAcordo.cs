namespace Modules.Financial.Domain.ValueObjects;

/// <summary>
/// Value Object com parâmetros para simulação de acordo de renegociação.
/// </summary>
public record ParametrosSimulacaoAcordo(
    decimal ValorTotalOriginal,
    decimal ValorDescontoConcedido,
    int QuantidadeParcelas,
    DateTime DataPrimeiroVencimento
);
