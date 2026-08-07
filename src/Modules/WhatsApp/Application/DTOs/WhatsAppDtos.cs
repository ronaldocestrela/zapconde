namespace Modules.WhatsApp.Application.DTOs;

public record WebhookIngestionResultDto(
    bool IsSuccess,
    bool IsDuplicate,
    int? WebhookLogId,
    string Message
);

public record WhatsAppWebhookLogDto(
    int Id,
    int TenantId,
    int CondoId,
    string InstanceName,
    string Provider,
    string MessageId,
    string SenderPhone,
    string? PushName,
    string MessageType,
    string? MessageText,
    string? MediaUrl,
    string Status,
    string? ErrorMessage,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? ProcessedAt,
    string RawPayloadJson,
    int? MoradorId = null
);

public record WhatsAppInstanceConfigDto(
    int Id,
    int TenantId,
    int CondoId,
    string InstanceName,
    string Provider,
    string BaseUrl,
    string ApiKey,
    string? WebhookSecret,
    bool IsActive,
    string Status,
    DateTimeOffset CriadoEm,
    DateTimeOffset? UltimaConexaoEm
);

public record WhatsAppWebhookSummaryDto(
    int TotalRecebidosHoje,
    int ProcessadosComSucesso,
    int Falhas,
    int IgnoradosIdempotencia,
    int InstanciasAtivas
);

public record CreateWhatsAppInstanceCommand(
    int CondoId,
    string InstanceName,
    string Provider,
    string BaseUrl,
    string ApiKey,
    string? WebhookSecret
);
