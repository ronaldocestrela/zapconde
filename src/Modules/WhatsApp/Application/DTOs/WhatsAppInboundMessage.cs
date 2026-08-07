using Modules.WhatsApp.Domain.Enums;

namespace Modules.WhatsApp.Application.DTOs;

/// <summary>
/// Objeto unificado normalizado extraído de qualquer payload de webhook de WhatsApp.
/// </summary>
public record WhatsAppInboundMessage(
    string InstanceName,
    WhatsAppProvider Provider,
    string MessageId,
    string SenderPhone,
    string? PushName,
    WhatsAppMessageType MessageType,
    string? MessageText,
    string? MediaUrl,
    bool FromMe,
    DateTimeOffset Timestamp,
    string RawJson
);
