namespace Modules.WhatsApp.Domain.Enums;

/// <summary>
/// Status do ciclo de vida e processamento de um webhook recebido.
/// </summary>
public enum WhatsAppWebhookStatus
{
    Received = 1,
    Queued = 2,
    Processed = 3,
    Failed = 4,
    Ignored = 5
}
