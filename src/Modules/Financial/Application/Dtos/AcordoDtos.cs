using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Application.Dtos;

public record SimulacaoAcordoRequest(
    int UnidadeId,
    List<int> FaturasIds,
    decimal ValorDescontoConcedido,
    int QuantidadeParcelas,
    DateTime DataPrimeiroVencimento
);

public record ProjecaoParcelaDto(
    int NumeroParcela,
    DateTime DataVencimento,
    decimal ValorParcela
);

public record SimulacaoAcordoResponse(
    decimal ValorTotalOriginal,
    decimal ValorDesconto,
    decimal ValorTotalAcordo,
    int QuantidadeParcelas,
    decimal ValorParcelaBase,
    List<ProjecaoParcelaDto> Parcelas
);

public record CriarAcordoRequest(
    int CondoId,
    int UnidadeId,
    int MoradorId,
    List<int> FaturasIds,
    decimal ValorDescontoConcedido,
    int QuantidadeParcelas,
    DateTime DataPrimeiroVencimento,
    string Observacoes = ""
);

public record ParcelaAcordoDto(
    int Id,
    int NumeroParcela,
    DateTime DataVencimento,
    decimal ValorParcela,
    StatusParcelaAcordo Status,
    DateTime? DataPagamento,
    int? FaturaGeradaId
);

public record AcordoFaturaVinculadaDto(
    int FaturaId,
    decimal ValorFaturaOriginal
);

public record AcordoDto(
    int Id,
    int TenantId,
    int CondoId,
    int UnidadeId,
    int MoradorId,
    string NumeroAcordo,
    DateTime DataCriacao,
    DateTime? DataAceite,
    DateTime DataPrimeiroVencimento,
    decimal ValorTotalOriginal,
    decimal ValorDesconto,
    decimal ValorTotalAcordo,
    int QuantidadeParcelas,
    StatusAcordo Status,
    string Observacoes,
    List<ParcelaAcordoDto> Parcelas,
    List<AcordoFaturaVinculadaDto> FaturasVinculadas
);
