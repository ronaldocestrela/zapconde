using BuildingBlocks.Shared.Messaging;
using BuildingBlocks.Shared.MultiTenancy;

namespace BuildingBlocks.Shared.Events;

/// <summary>
/// Evento de integração disparado após o processamento downstream da mensagem recebida do WhatsApp,
/// contendo os metadados resolvidos de Tenant, Condomínio e Morador (caso identificado).
/// </summary>
public record WhatsAppMessageProcessedEvent : IIntegrationEvent, ITenantScoped
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    public int TenantId { get; set; }
    public int CondoId { get; init; }
    public int WebhookLogId { get; init; }
    public int? MoradorId { get; init; }
    public Guid? UserId { get; init; }

    public string InstanceName { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string MessageId { get; init; } = string.Empty;
    public string SenderPhone { get; init; } = string.Empty;
    public string? PushName { get; init; }
    public string MessageType { get; init; } = "Text";
    public string? MessageText { get; init; }
    public string? MediaUrl { get; init; }
    public bool IsResidentIdentified { get; init; }
    public bool CacheHit { get; init; }
}
