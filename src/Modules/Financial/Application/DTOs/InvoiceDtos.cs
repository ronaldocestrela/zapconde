using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Application.DTOs;

public record ItemCobrancaDto(
    int Id,
    string Descricao,
    TipoItemCobranca Tipo,
    string TipoDescricao,
    decimal ValorUnitario,
    int Quantidade,
    decimal Subtotal
);

public record BoletoDto(
    int Id,
    string NossoNumero,
    string LinhaDigitavel,
    string CodigoBarras,
    string CodigoPixCopiaECola,
    string QrCodeUrl,
    string PdfUrl,
    decimal Valor,
    DateTime DataVencimento,
    DateTime DataEmissao,
    DateTime? DataPagamento,
    StatusBoleto Status,
    string StatusDescricao
);

public record FaturaSummaryDto(
    int Id,
    int CondoId,
    int UnidadeId,
    string BlocoNumeroUnidade, // Ex: "Bloco A - 101" ou "Unidade 101"
    int MoradorId,
    string NomeMorador,
    string Competencia,
    string NumeroFatura,
    DateTime DataEmissao,
    DateTime DataVencimento,
    decimal ValorOriginal,
    decimal ValorDesconto,
    decimal ValorMulta,
    decimal ValorJuros,
    decimal TotalFinal,
    StatusFatura Status,
    string StatusDescricao,
    DateTime? DataPagamento,
    bool TemBoleto
);

public record FaturaDetailDto(
    int Id,
    int CondoId,
    int UnidadeId,
    string BlocoNumeroUnidade,
    int MoradorId,
    string NomeMorador,
    string Competencia,
    string NumeroFatura,
    DateTime DataEmissao,
    DateTime DataVencimento,
    decimal ValorOriginal,
    decimal ValorDesconto,
    decimal ValorMulta,
    decimal ValorJuros,
    decimal TotalFinal,
    StatusFatura Status,
    string StatusDescricao,
    DateTime? DataPagamento,
    string Observacoes,
    IEnumerable<ItemCobrancaDto> Itens,
    BoletoDto? Boleto
);

public record CreateItemCobrancaRequest(
    string Descricao,
    TipoItemCobranca Tipo,
    decimal ValorUnitario,
    int Quantidade = 1
);

public record CreateFaturaRequest(
    int CondoId,
    int UnidadeId,
    int MoradorId,
    string Competencia,
    DateTime DataVencimento,
    string Observacoes,
    List<CreateItemCobrancaRequest> Itens
);
