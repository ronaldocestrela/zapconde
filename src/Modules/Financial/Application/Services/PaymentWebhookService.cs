using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Infrastructure.Persistence;

namespace Modules.Financial.Application.Services;

public interface IPaymentWebhookService
{
    Task<Result<string>> ProcessAsaasWebhookAsync(
        AsaasWebhookEventDto webhookEvent,
        string accessTokenHeader,
        CancellationToken ct = default);
}

public class PaymentWebhookService : IPaymentWebhookService
{
    private readonly FinancialDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentWebhookService> _logger;

    public PaymentWebhookService(
        FinancialDbContext dbContext,
        ICacheService cacheService,
        IConfiguration configuration,
        ILogger<PaymentWebhookService> logger)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<string>> ProcessAsaasWebhookAsync(
        AsaasWebhookEventDto webhookEvent,
        string accessTokenHeader,
        CancellationToken ct = default)
    {
        if (webhookEvent == null || webhookEvent.Payment == null)
        {
            return Result<string>.ValidationFailure(new[] { "Payload de webhook inválido." });
        }

        // Validação de Token de Acesso (X-Asaas-Access-Token)
        var expectedToken = _configuration["Financial:Asaas:WebhookToken"] ?? "zapconde-webhook-secret-token";
        if (!string.IsNullOrWhiteSpace(expectedToken) && !expectedToken.Equals(accessTokenHeader, StringComparison.Ordinal))
        {
            _logger.LogWarning("Tentativa de Webhook com token de acesso inválido. Recebido: {ReceivedToken}", accessTokenHeader);
            return Result<string>.Failure("Token de segurança do Webhook inválido.");
        }

        // Verificação de Idempotência no Cache (Redis)
        var idempotencyKey = $"financial:webhook:asaas:{webhookEvent.Id}";
        var alreadyProcessed = await _cacheService.GetAsync<bool>(idempotencyKey, ct);
        if (alreadyProcessed)
        {
            _logger.LogInformation("Webhook {EventId} já processado anteriormente. Ignorando evento duplicado.", webhookEvent.Id);
            return Result<string>.Success("Evento duplicado ignorado com sucesso (idempotente).");
        }

        var externalChargeId = webhookEvent.Payment.Id;
        var eventType = webhookEvent.Event?.ToUpperInvariant() ?? string.Empty;

        // Ignora filtro global de tenant para localizar a fatura pelo ExternalChargeId vindo do webhook
        var boleto = await _dbContext.Boletos
            .IgnoreQueryFilters()
            .Include(b => b.Fatura)
            .FirstOrDefaultAsync(b => b.ExternalChargeId == externalChargeId, ct);

        if (boleto == null)
        {
            _logger.LogWarning("Cobrança externa {ExternalChargeId} não encontrada no banco de dados local.", externalChargeId);
            // Armazena evento para evitar retentativas desnecessárias
            await _cacheService.SetAsync(idempotencyKey, true, TimeSpan.FromHours(24), ct);
            return Result<string>.Success("Cobrança não encontrada no ambiente local.");
        }

        switch (eventType)
        {
            case "PAYMENT_RECEIVED":
            case "PAYMENT_CONFIRMED":
            case "PAYMENT_RECEIVED_IN_CASH":
                var paymentDate = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(webhookEvent.Payment.PaymentDate) &&
                    DateTime.TryParse(webhookEvent.Payment.PaymentDate, out var parsedDate))
                {
                    paymentDate = parsedDate;
                }

                var valorPago = webhookEvent.Payment.Value > 0 ? webhookEvent.Payment.Value : boleto.Valor;

                boleto.RegistrarPagamento(paymentDate);
                if (boleto.Fatura != null)
                {
                    boleto.Fatura.RegistrarPagamento(paymentDate, valorPago);
                }
                _logger.LogInformation("Pagamento da fatura #{FaturaId} confirmado via Webhook. Valor: {Valor}", boleto.FaturaId, valorPago);
                break;

            case "PAYMENT_DELETED":
            case "PAYMENT_REFUNDED":
                boleto.Cancelar();
                _logger.LogInformation("Cobrança #{ExternalChargeId} cancelada via Webhook.", externalChargeId);
                break;
        }

        await _dbContext.SaveChangesAsync(ct);

        // Registra idempotência no Redis por 24h
        await _cacheService.SetAsync(idempotencyKey, true, TimeSpan.FromHours(24), ct);

        return Result<string>.Success("Webhook processado e fatura conciliada com sucesso.");
    }
}
