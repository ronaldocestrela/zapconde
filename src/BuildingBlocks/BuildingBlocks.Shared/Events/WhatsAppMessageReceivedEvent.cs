using BuildingBlocks.Shared.Messaging;
using BuildingBlocks.Shared.MultiTenancy;

namespace BuildingBlocks.Shared.Events;

/// <summary>
/// Evento de integração disparado após a ingestão e validação com sucesso de um webhook do WhatsApp.
/// Publicado assincronamente via MassTransit Outbox para consumo downstream (AI Orchestrator / Processador de Fluxos).
/// </summary>
public record WhatsAppMessageReceivedEvent : IIntegrationEvent, ITenantScoped
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    public int TenantId { get; set; }
    public int CondoId { get; init; }
    public int WebhookLogId { get; init; }

    public string InstanceName { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string MessageId { get; init; } = string.Empty;
    public string SenderPhone { get; init; } = string.Empty;
    public string? PushName { get; init; }
    public string MessageType { get; init; } = "Text";
    public string? MessageText { get; init; }
    public string? MediaUrl { get; init; }
    public string RawPayloadJson { get; init; } = string.Empty;
}
