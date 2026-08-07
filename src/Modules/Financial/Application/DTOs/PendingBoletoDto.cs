using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Application.DTOs;

/// <summary>
/// DTO estruturado representando os dados de um boleto pendente para consumo de Function Calling / Plugins de IA.
/// </summary>
public record PendingBoletoDto(
    int FaturaId,
    int? BoletoId,
    int MoradorId,
    int UnidadeId,
    string Competencia,
    string NumeroFatura,
    decimal ValorTotal,
    DateTime DataVencimento,
    StatusFatura StatusFatura,
    string StatusFaturaDescricao,
    string CodigoPixCopiaECola,
    string LinhaDigitavel,
    string CodigoBarras,
    string PdfUrl,
    bool Vencido
);

public record BoletoPluginExecutionResultDto(
    int MoradorId,
    int QuantidadePendencias,
    decimal ValorTotalPendencias,
    IEnumerable<PendingBoletoDto> Boletos,
    string MensagemFormatadaFormatadaIa
);
