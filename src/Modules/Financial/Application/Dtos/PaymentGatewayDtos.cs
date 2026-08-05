using System.Text.Json.Serialization;
using BuildingBlocks.Shared.Enums;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Application.Dtos;

public record BoletoCobrancaRequestDto(
    int FaturaId,
    decimal Valor,
    DateTime DataVencimento,
    string MoradorNome,
    string MoradorCpfCnpj,
    string? MoradorEmail = null,
    string? MoradorTelefone = null,
    string? Descricao = null
);

public record GatewayEmissaoResultadoDto(
    string ExternalChargeId,
    PaymentGatewayProvider Provider,
    string LinhaDigitavel,
    string CodigoBarras,
    string CodigoPixCopiaECola,
    string PixQrCodeBase64,
    string PdfUrl,
    StatusBoleto Status,
    DateTime DataVencimento,
    decimal Valor
);

public record GatewayCobrancaStatusDto(
    string ExternalChargeId,
    GatewayChargeStatus Status,
    DateTime? DataPagamento,
    decimal? ValorPago
);

public record PaymentInfoResponseDto(
    int FaturaId,
    int? BoletoId,
    string ExternalChargeId,
    PaymentGatewayProvider Provider,
    string LinhaDigitavel,
    string CodigoBarras,
    string CodigoPixCopiaECola,
    string PixQrCodeBase64,
    string PdfUrl,
    string StatusBoleto,
    string StatusFatura,
    decimal ValorOriginal,
    decimal TotalFinal,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    DateTime? DataUltimaSincronizacao
);

/// <summary>
/// Estrutura do payload de evento de Webhook enviado pelo Asaas.
/// </summary>
public class AsaasWebhookEventDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("dateCreated")]
    public string DateCreated { get; set; } = string.Empty;

    [JsonPropertyName("payment")]
    public AsaasPaymentDetailsDto? Payment { get; set; }
}

public class AsaasPaymentDetailsDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("customer")]
    public string Customer { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("netValue")]
    public decimal NetValue { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("billingType")]
    public string BillingType { get; set; } = string.Empty;

    [JsonPropertyName("paymentDate")]
    public string? PaymentDate { get; set; }

    [JsonPropertyName("clientPaymentDate")]
    public string? ClientPaymentDate { get; set; }

    [JsonPropertyName("invoiceUrl")]
    public string? InvoiceUrl { get; set; }

    [JsonPropertyName("bankSlipUrl")]
    public string? BankSlipUrl { get; set; }
}
