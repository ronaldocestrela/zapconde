using System.Text;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Enums;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Infrastructure.Services;

/// <summary>
/// Provedor Stub/Mock de Gateway de Pagamento para testes e ambiente de desenvolvimento.
/// Gera payloads PIX Copia e Cola válidos, QR Codes Base64 e Linhas Digitáveis formatadas.
/// </summary>
public class MockPaymentGatewayService : IPaymentGatewayService
{
    private static readonly Dictionary<string, GatewayCobrancaStatusDto> _simulatedCharges = new();

    public Task<Result<GatewayEmissaoResultadoDto>> GerarCobrancaBoletoPixAsync(
        BoletoCobrancaRequestDto request, CancellationToken ct = default)
    {
        if (request.Valor <= 0)
        {
            return Task.FromResult(Result<GatewayEmissaoResultadoDto>.ValidationFailure(
                new[] { "Valor da cobrança deve ser maior que zero." }));
        }

        var chargeId = $"pay_mock_{request.FaturaId}_{Guid.NewGuid():N}[..10]";
        var linhaDigitavel = $"34191.79001 01043.510047 91020.150008 8 {request.DataVencimento:yyMMdd}{((int)(request.Valor * 100)):D8}";
        var codigoBarras = $"34198{request.DataVencimento:yyMMdd}{((int)(request.Valor * 100)):D8}17900101043510049102015000";
        var pixCopiaECola = $"00020126580014BR.GOV.BCB.PIX0136{Guid.NewGuid()}52040000530398654{request.Valor:F2}5802BR5915SmartCondo SaaS6009SAO PAULO62070503***6304ABCD";
        
        // Simula uma imagem QR Code SVG minimalista em Base64
        var svgText = $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"200\" height=\"200\"><rect width=\"100%\" height=\"100%\" fill=\"#2E5B88\"/><text x=\"50%\" y=\"50%\" fill=\"#FFFFFF\" font-size=\"14\" text-anchor=\"middle\">PIX {request.Valor:C2}</text></svg>";
        var qrCodeBase64 = $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(svgText))}";
        var pdfUrl = $"https://sandbox.zapcondo.com.br/boletos/pdf/{chargeId}.pdf";

        var result = new GatewayEmissaoResultadoDto(
            ExternalChargeId: chargeId,
            Provider: PaymentGatewayProvider.Mock,
            LinhaDigitavel: linhaDigitavel,
            CodigoBarras: codigoBarras,
            CodigoPixCopiaECola: pixCopiaECola,
            PixQrCodeBase64: qrCodeBase64,
            PdfUrl: pdfUrl,
            Status: StatusBoleto.Gerado,
            DataVencimento: request.DataVencimento,
            Valor: request.Valor
        );

        _simulatedCharges[chargeId] = new GatewayCobrancaStatusDto(
            chargeId,
            GatewayChargeStatus.Pending,
            null,
            null
        );

        return Task.FromResult(Result<GatewayEmissaoResultadoDto>.Success(result));
    }

    public Task<Result<GatewayCobrancaStatusDto>> ConsultarStatusCobrancaAsync(
        string externalChargeId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(externalChargeId))
        {
            return Task.FromResult(Result<GatewayCobrancaStatusDto>.ValidationFailure(
                new[] { "ID da cobrança externa é obrigatório." }));
        }

        if (_simulatedCharges.TryGetValue(externalChargeId, out var status))
        {
            return Task.FromResult(Result<GatewayCobrancaStatusDto>.Success(status));
        }

        // Se não registrado previamente na memória estática, retorna um status padrão pendente
        var defaultStatus = new GatewayCobrancaStatusDto(
            externalChargeId,
            GatewayChargeStatus.Pending,
            null,
            null
        );

        return Task.FromResult(Result<GatewayCobrancaStatusDto>.Success(defaultStatus));
    }

    public Task<Result<bool>> CancelarCobrancaAsync(
        string externalChargeId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(externalChargeId))
        {
            return Task.FromResult(Result<bool>.ValidationFailure(
                new[] { "ID da cobrança externa é obrigatório." }));
        }

        if (_simulatedCharges.ContainsKey(externalChargeId))
        {
            _simulatedCharges[externalChargeId] = new GatewayCobrancaStatusDto(
                externalChargeId,
                GatewayChargeStatus.Canceled,
                null,
                null
            );
        }

        return Task.FromResult(Result<bool>.Success(true));
    }

    public static void SimulatePayment(string externalChargeId, decimal valorPago, DateTime dataPagamento)
    {
        _simulatedCharges[externalChargeId] = new GatewayCobrancaStatusDto(
            externalChargeId,
            GatewayChargeStatus.Confirmed,
            dataPagamento,
            valorPago
        );
    }
}
