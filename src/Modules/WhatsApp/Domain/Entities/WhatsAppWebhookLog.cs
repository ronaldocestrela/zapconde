using BuildingBlocks.Shared.MultiTenancy;
using Modules.WhatsApp.Domain.Enums;
using Modules.WhatsApp.Domain.Exceptions;

namespace Modules.WhatsApp.Domain.Entities;

/// <summary>
/// Entidade Aggregate Root que registra payloads de Webhooks do WhatsApp recebidos com rastreabilidade,
/// isolamento multi-tenant (ITenantScoped) e idempotência.
/// </summary>
public class WhatsAppWebhookLog : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public string InstanceName { get; private set; } = string.Empty;
    public WhatsAppProvider Provider { get; private set; }
    public string MessageId { get; private set; } = string.Empty;
    public string SenderPhone { get; private set; } = string.Empty;
    public string? PushName { get; private set; }
    public WhatsAppMessageType MessageType { get; private set; }
    public string? MessageText { get; private set; }
    public string? MediaUrl { get; private set; }
    public string RawPayloadJson { get; private set; } = string.Empty;
    public WhatsAppWebhookStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int? MoradorId { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; private set; }

    // Construtor EF Core
    private WhatsAppWebhookLog() { }

    /// <summary>
    /// Factory Method para criar um log de Webhook recebido.
    /// </summary>
    public static WhatsAppWebhookLog Registrar(
        int tenantId,
        int condoId,
        string instanceName,
        WhatsAppProvider provider,
        string messageId,
        string senderPhone,
        string? pushName,
        WhatsAppMessageType messageType,
        string? messageText,
        string? mediaUrl,
        string rawPayloadJson)
    {
        if (tenantId <= 0)
            throw new WhatsAppDomainException("TenantId é obrigatório.");

        if (condoId <= 0)
            throw new WhatsAppDomainException("CondoId é obrigatório.");

        if (string.IsNullOrWhiteSpace(instanceName))
            throw new WhatsAppDomainException("O nome da instância é obrigatório.");

        if (string.IsNullOrWhiteSpace(messageId))
            throw new WhatsAppDomainException("O MessageId (identificador único da mensagem) é obrigatório.");

        if (string.IsNullOrWhiteSpace(senderPhone))
            throw new WhatsAppDomainException("O telefone do remetente é obrigatório.");

        if (string.IsNullOrWhiteSpace(rawPayloadJson))
            throw new WhatsAppDomainException("O payload bruto em JSON é obrigatório.");

        return new WhatsAppWebhookLog
        {
            TenantId = tenantId,
            CondoId = condoId,
            InstanceName = instanceName.Trim(),
            Provider = provider,
            MessageId = messageId.Trim(),
            SenderPhone = NormalizarTelefone(senderPhone),
            PushName = pushName?.Trim(),
            MessageType = messageType,
            MessageText = messageText?.Trim(),
            MediaUrl = mediaUrl?.Trim(),
            RawPayloadJson = rawPayloadJson,
            Status = WhatsAppWebhookStatus.Received,
            ReceivedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Marca o log como processado com sucesso.
    /// </summary>
    public void MarcarComoProcessado(int? moradorId = null)
    {
        Status = WhatsAppWebhookStatus.Processed;
        MoradorId = moradorId;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marca o log como ignorado (ex: duplicidade por idempotência).
    /// </summary>
    public void MarcarComoIgnorado(string motivo)
    {
        Status = WhatsAppWebhookStatus.Ignored;
        ErrorMessage = motivo;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marca o log como falha no processamento.
    /// </summary>
    public void MarcarComoFalha(string erro)
    {
        Status = WhatsAppWebhookStatus.Failed;
        ErrorMessage = erro;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizarTelefone(string phone)
    {
        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        if (!digitsOnly.StartsWith('+'))
        {
            return "+" + digitsOnly;
        }
        return digitsOnly;
    }
}
