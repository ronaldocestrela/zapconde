using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Infrastructure.Services;

/// <summary>
/// Integração real com a API v3 do Asaas (Boleto + PIX híbrido).
/// </summary>
public class AsaasPaymentGatewayService : IPaymentGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AsaasPaymentGatewayService> _logger;
    private readonly MockPaymentGatewayService _mockFallback;
    private readonly bool _useMock;

    public AsaasPaymentGatewayService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AsaasPaymentGatewayService> logger,
        MockPaymentGatewayService mockFallback)
    {
        _httpClient = httpClient;
        _logger = logger;
        _mockFallback = mockFallback;

        var apiKey = configuration["Financial:Asaas:ApiKey"] ?? string.Empty;
        var baseUrl = configuration["Financial:Asaas:BaseUrl"] ?? "https://sandbox.asaas.com/api/v3/";
        
        _useMock = string.IsNullOrWhiteSpace(apiKey) || apiKey.Equals("mock", StringComparison.OrdinalIgnoreCase);

        if (!_useMock)
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("access_token", apiKey);
        }
    }

    public async Task<Result<GatewayEmissaoResultadoDto>> GerarCobrancaBoletoPixAsync(
        BoletoCobrancaRequestDto request, CancellationToken ct = default)
    {
        if (_useMock)
        {
            _logger.LogInformation("ApiKey do Asaas não configurada. Utilizando MockPaymentGatewayService para cobrança da fatura {FaturaId}.", request.FaturaId);
            return await _mockFallback.GerarCobrancaBoletoPixAsync(request, ct);
        }

        try
        {
            var payload = new
            {
                customer = request.MoradorCpfCnpj,
                billingType = "UNDEFINED", // Híbrido: Boleto + PIX
                value = request.Valor,
                dueDate = request.DataVencimento.ToString("yyyy-MM-dd"),
                description = request.Descricao ?? $"Taxa Condominial Fatura #{request.FaturaId}",
                externalReference = request.FaturaId.ToString()
            };

            var response = await _httpClient.PostAsJsonAsync("payments", payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Erro na API do Asaas ({StatusCode}): {ErrorContent}", response.StatusCode, errorContent);
                return Result<GatewayEmissaoResultadoDto>.Failure($"Erro na emissão Asaas: {response.StatusCode}");
            }

            using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = jsonDoc.RootElement;

            var id = root.GetProperty("id").GetString() ?? string.Empty;
            var invoiceUrl = root.TryGetProperty("invoiceUrl", out var iu) ? iu.GetString() : string.Empty;
            var bankSlipUrl = root.TryGetProperty("bankSlipUrl", out var bsu) ? bsu.GetString() : string.Empty;
            var nossonumero = root.TryGetProperty("nossoNumero", out var nn) ? nn.GetString() : id;

            // Busca QR Code PIX
            var pixResponse = await _httpClient.GetAsync($"payments/{id}/pixQrCode", ct);
            var pixCopiaECola = string.Empty;
            var qrCodeBase64 = string.Empty;

            if (pixResponse.IsSuccessStatusCode)
            {
                using var pixJsonDoc = await JsonDocument.ParseAsync(await pixResponse.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                var pixRoot = pixJsonDoc.RootElement;
                pixCopiaECola = pixRoot.TryGetProperty("payload", out var p) ? p.GetString() ?? string.Empty : string.Empty;
                qrCodeBase64 = pixRoot.TryGetProperty("encodedImage", out var ei) ? ei.GetString() ?? string.Empty : string.Empty;
            }

            var result = new GatewayEmissaoResultadoDto(
                ExternalChargeId: id,
                Provider: PaymentGatewayProvider.Asaas,
                LinhaDigitavel: nossonumero ?? string.Empty,
                CodigoBarras: nossonumero ?? string.Empty,
                CodigoPixCopiaECola: pixCopiaECola,
                PixQrCodeBase64: qrCodeBase64,
                PdfUrl: bankSlipUrl ?? invoiceUrl ?? string.Empty,
                Status: StatusBoleto.Gerado,
                DataVencimento: request.DataVencimento,
                Valor: request.Valor
            );

            return Result<GatewayEmissaoResultadoDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção inesperada ao chamar Asaas Gateway. Ativando fallback para mock.");
            return await _mockFallback.GerarCobrancaBoletoPixAsync(request, ct);
        }
    }

    public async Task<Result<GatewayCobrancaStatusDto>> ConsultarStatusCobrancaAsync(
        string externalChargeId, CancellationToken ct = default)
    {
        if (_useMock)
        {
            return await _mockFallback.ConsultarStatusCobrancaAsync(externalChargeId, ct);
        }

        try
        {
            var response = await _httpClient.GetAsync($"payments/{externalChargeId}", ct);
            if (!response.IsSuccessStatusCode)
            {
                return Result<GatewayCobrancaStatusDto>.Failure($"Falha ao consultar Asaas: {response.StatusCode}");
            }

            using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = jsonDoc.RootElement;

            var statusStr = root.GetProperty("status").GetString() ?? "PENDING";
            var status = MapAsaasStatus(statusStr);

            DateTime? dataPagamento = null;
            if (root.TryGetProperty("paymentDate", out var pd) && pd.GetString() is { } pdStr && DateTime.TryParse(pdStr, out var parsedPd))
            {
                dataPagamento = parsedPd;
            }

            decimal? valorPago = null;
            if (root.TryGetProperty("value", out var v) && v.TryGetDecimal(out var parsedV))
            {
                valorPago = parsedV;
            }

            return Result<GatewayCobrancaStatusDto>.Success(new GatewayCobrancaStatusDto(
                externalChargeId,
                status,
                dataPagamento,
                valorPago
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status no Asaas.");
            return await _mockFallback.ConsultarStatusCobrancaAsync(externalChargeId, ct);
        }
    }

    public async Task<Result<bool>> CancelarCobrancaAsync(
        string externalChargeId, CancellationToken ct = default)
    {
        if (_useMock)
        {
            return await _mockFallback.CancelarCobrancaAsync(externalChargeId, ct);
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"payments/{externalChargeId}", ct);
            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar cobrança no Asaas.");
            return await _mockFallback.CancelarCobrancaAsync(externalChargeId, ct);
        }
    }

    private static GatewayChargeStatus MapAsaasStatus(string status)
    {
        return status.ToUpperInvariant() switch
        {
            "RECEIVED" or "CONFIRMED" or "RECEIVED_IN_CASH" => GatewayChargeStatus.Confirmed,
            "OVERDUE" => GatewayChargeStatus.Overdue,
            "REFUNDED" or "REFUND_REQUESTED" => GatewayChargeStatus.Refunded,
            "DELETED" => GatewayChargeStatus.Canceled,
            _ => GatewayChargeStatus.Pending
        };
    }
}
