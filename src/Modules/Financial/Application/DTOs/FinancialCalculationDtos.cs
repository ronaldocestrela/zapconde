namespace Modules.Financial.Application.DTOs;

/// <summary>
/// DTO de requisição de simulação financeira ad-hoc.
/// </summary>
public record SimularCalculoRequestDto(
    decimal ValorOriginal,
    DateTime DataVencimento,
    DateTime DataSimulacao,
    decimal PercentualMulta = 2.0m,
    decimal PercentualJurosMensal = 1.0m,
    decimal ValorDescontoPontualidade = 0m,
    decimal PercentualDescontoPontualidade = 0m,
    DateTime? DataLimiteDesconto = null
);

/// <summary>
/// DTO de resultado detalhado do cálculo/simulação financeira.
/// </summary>
public record CalculoFinanceiroDto(
    decimal ValorOriginal,
    DateTime DataVencimento,
    DateTime DataCalculo,
    int DiasAtraso,
    decimal ValorMulta,
    decimal ValorJuros,
    decimal ValorDesconto,
    decimal ValorTotalCalculado,
    string MemoriaCalculoTextual
);

/// <summary>
/// DTO para projeção futura de pagamentos em atraso.
/// </summary>
public record ProjecaoCalculoDto(
    int DiasAtrasoAdicionais,
    DateTime DataProjecao,
    decimal ValorOriginal,
    decimal ValorMulta,
    decimal ValorJuros,
    decimal ValorDesconto,
    decimal ValorTotalProjetado
);
