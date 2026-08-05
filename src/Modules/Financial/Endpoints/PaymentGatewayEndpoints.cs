using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Application.Services;

namespace Modules.Financial.Endpoints;

public record GeneratePaymentRequest(int Id);

/// <summary>
/// Endpoint para gerar cobrança (Boleto + PIX) de uma fatura no gateway externo.
/// </summary>
public sealed class GeneratePaymentEndpoint : Endpoint<GeneratePaymentRequest, Result<PaymentInfoResponseDto>>
{
    private readonly IInvoicePaymentApplicationService _paymentService;

    public GeneratePaymentEndpoint(IInvoicePaymentApplicationService paymentService)
    {
        _paymentService = paymentService;
    }

    public override void Configure()
    {
        Post("/api/financial/invoices/{id}/generate-payment");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Gerar Boleto e PIX via Gateway";
            s.Description = "Emite a cobrança no gateway externo (Asaas/Mock), vincula a linha digitável, QR Code e salva no banco.";
        });
    }

    public override async Task HandleAsync(GeneratePaymentRequest req, CancellationToken ct)
    {
        var result = await _paymentService.GeneratePaymentAsync(req.Id, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
        }
        else
        {
            var status = result.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
            await SendAsync(result, status, ct);
        }
    }
}

public record GetPaymentInfoRequest(int Id);

/// <summary>
/// Endpoint para consultar informações detalhadas de pagamento (PIX, QR Code Base64, Linha Digitável e PDF URL).
/// </summary>
public sealed class GetPaymentInfoEndpoint : Endpoint<GetPaymentInfoRequest, Result<PaymentInfoResponseDto>>
{
    private readonly IInvoicePaymentApplicationService _paymentService;

    public GetPaymentInfoEndpoint(IInvoicePaymentApplicationService paymentService)
    {
        _paymentService = paymentService;
    }

    public override void Configure()
    {
        Get("/api/financial/invoices/{id}/payment-info");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter dados de pagamento da fatura";
            s.Description = "Retorna chave PIX Copia e Cola, QR Code visual em Base64, Linha Digitável e link do PDF do boleto.";
        });
    }

    public override async Task HandleAsync(GetPaymentInfoRequest req, CancellationToken ct)
    {
        var result = await _paymentService.GetPaymentInfoAsync(req.Id, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
        }
        else
        {
            var status = result.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
            await SendAsync(result, status, ct);
        }
    }
}

public record SyncPaymentRequest(int Id);

/// <summary>
/// Endpoint para acionar a sincronização sob demanda do status de pagamento com o gateway externo.
/// </summary>
public sealed class SyncPaymentEndpoint : Endpoint<SyncPaymentRequest, Result<PaymentInfoResponseDto>>
{
    private readonly IInvoicePaymentApplicationService _paymentService;

    public SyncPaymentEndpoint(IInvoicePaymentApplicationService paymentService)
    {
        _paymentService = paymentService;
    }

    public override void Configure()
    {
        Post("/api/financial/invoices/{id}/sync-payment");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Sincronizar status de pagamento sob demanda";
            s.Description = "Consulta o gateway externo para verificar a liquidação da cobrança e atualizar os dados da fatura.";
        });
    }

    public override async Task HandleAsync(SyncPaymentRequest req, CancellationToken ct)
    {
        var result = await _paymentService.SyncPaymentAsync(req.Id, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
        }
        else
        {
            var status = result.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
            await SendAsync(result, status, ct);
        }
    }
}

/// <summary>
/// Endpoint público para recebimento de Webhooks do Asaas.
/// Valida o header 'X-Asaas-Access-Token' e executa o processamento idempotente via Redis.
/// </summary>
public sealed class AsaasWebhookEndpoint : Endpoint<AsaasWebhookEventDto, Result<string>>
{
    private readonly IPaymentWebhookService _webhookService;

    public AsaasWebhookEndpoint(IPaymentWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    public override void Configure()
    {
        Post("/api/financial/webhooks/asaas");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Webhook de Notificações do Asaas";
            s.Description = "Recebe webhooks de eventos de pagamento (PAYMENT_RECEIVED, etc.) com idempotência via Redis.";
        });
    }

    public override async Task HandleAsync(AsaasWebhookEventDto req, CancellationToken ct)
    {
        var accessTokenHeader = HttpContext.Request.Headers["X-Asaas-Access-Token"].ToString();

        var result = await _webhookService.ProcessAsaasWebhookAsync(req, accessTokenHeader, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
        }
        else
        {
            var status = result.Message.Contains("inválido", StringComparison.OrdinalIgnoreCase) ? 401 : 400;
            await SendAsync(result, status, ct);
        }
    }
}
