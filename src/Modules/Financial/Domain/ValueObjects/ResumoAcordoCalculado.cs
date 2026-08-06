namespace Modules.Financial.Domain.ValueObjects;

/// <summary>
/// Projeção de valores da parcela no cálculo do acordo.
/// </summary>
public record ProjecaoParcelaAcordo(
    int NumeroParcela,
    DateTime DataVencimento,
    decimal ValorParcela
);

/// <summary>
/// Value Object com resultado consolidado da simulação do acordo.
/// </summary>
public record ResumoAcordoCalculado(
    decimal ValorTotalOriginal,
    decimal ValorDesconto,
    decimal ValorTotalAcordo,
    int QuantidadeParcelas,
    decimal ValorParcelaBase,
    IReadOnlyList<ProjecaoParcelaAcordo> Parcelas
);
